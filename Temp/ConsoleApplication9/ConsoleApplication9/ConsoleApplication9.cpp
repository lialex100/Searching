// ConsoleApplication9.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "ConsoleApplication9.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// The one and only application object

CWinApp theApp;

using namespace std;
#include <Windows.h>
#include <WinIoCtl.h>
#include <stdio.h>

#define BUF_LEN 4096

#include <Windows.h>

#include <stdio.h>

#define BUFFER_SIZE (1024 * 1024)

HANDLE drive;
USN maxusn;
///http://stackoverflow.com/questions/7421440/how-can-i-detect-only-deleted-changed-and-created-files-on-a-volume
void show_record(USN_RECORD * record)
{
	void * buffer;
	MFT_ENUM_DATA mft_enum_data;
	DWORD bytecount = 1;
	USN_RECORD * parent_record;

	WCHAR * filename;
	WCHAR * filenameend;

	printf("=================================================================\n");
	printf("RecordLength: %u\n", record->RecordLength);
	printf("MajorVersion: %u\n", (DWORD)record->MajorVersion);
	printf("MinorVersion: %u\n", (DWORD)record->MinorVersion);
	printf("FileReferenceNumber: %llu\n", record->FileReferenceNumber);
	printf("ParentFRN: %llu\n", record->ParentFileReferenceNumber);
	printf("USN: %lu\n", record->Usn);
	printf("Timestamp: %lu\n", record->TimeStamp);
	printf("Reason: %u\n", record->Reason);
	printf("SourceInfo: %u\n", record->SourceInfo);
	printf("SecurityId: %u\n", record->SecurityId);
	printf("FileAttributes: %x\n", record->FileAttributes);
	printf("FileNameLength: %u\n", (DWORD)record->FileNameLength);

	filename = (WCHAR *)(((BYTE *)record) + record->FileNameOffset);
	filenameend = (WCHAR *)(((BYTE *)record) + record->FileNameOffset + record->FileNameLength);

	printf("FileName: %.*ls\n", filenameend - filename, filename);

	buffer = VirtualAlloc(NULL, BUFFER_SIZE, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);

	if (buffer == NULL)
	{
		printf("VirtualAlloc: %u\n", GetLastError());
		return;
	}

	mft_enum_data.StartFileReferenceNumber = record->ParentFileReferenceNumber;
	mft_enum_data.LowUsn = 0;
	mft_enum_data.HighUsn = maxusn;

	if (!DeviceIoControl(drive, FSCTL_ENUM_USN_DATA, &mft_enum_data, sizeof(mft_enum_data), buffer, BUFFER_SIZE, &bytecount, NULL))
	{
		printf("FSCTL_ENUM_USN_DATA (show_record): %u\n", GetLastError());
		return;
	}

	parent_record = (USN_RECORD *)((USN *)buffer + 1);

	if (parent_record->FileReferenceNumber != record->ParentFileReferenceNumber)
	{
		printf("=================================================================\n");
		printf("Couldn't retrieve FileReferenceNumber %u\n", record->ParentFileReferenceNumber);
		return;
	}

	show_record(parent_record);
}

void check_record(USN_RECORD * record)
{
	WCHAR * filename;
	WCHAR * filenameend;

	filename = (WCHAR *)(((BYTE *)record) + record->FileNameOffset);
	filenameend = (WCHAR *)(((BYTE *)record) + record->FileNameOffset + record->FileNameLength);
	//	printf("- - - - - - - - FileName: %.*ls\n", filenameend - filename, filename);
	if (filenameend - filename != 8) return;

	if (wcsncmp(filename, L"test.txt", 8) != 0) return;

	show_record(record);
}






int main(int argc, char ** argv)
{

	HANDLE hDir = CreateFileW(L"\\\\?\\c:\\", FILE_LIST_DIRECTORY, FILE_SHARE_READ | FILE_SHARE_WRITE, nullptr, OPEN_EXISTING, FILE_FLAG_BACKUP_SEMANTICS, nullptr);

	if (hDir == INVALID_HANDLE_VALUE) {
		wcout << L"Failed to open directory" << endl;
		return 1;
	}

	DWORD dwBufferLength = 10000;
	BYTE *pBuffer = new BYTE[dwBufferLength];

	while (true) {
		DWORD dwBytesReturned;

		if (!ReadDirectoryChangesW(hDir, pBuffer, dwBufferLength, TRUE, FILE_NOTIFY_CHANGE_FILE_NAME  | FILE_NOTIFY_CHANGE_DIR_NAME, &dwBytesReturned, nullptr, nullptr)) {
			wcout << L"Failed to read directory changes" << endl;
			break;
		}

		FILE_NOTIFY_INFORMATION *info = reinterpret_cast<FILE_NOTIFY_INFORMATION *>(pBuffer);

		do {

			if (info->Action == FILE_ACTION_ADDED | info->Action ==  FILE_ACTION_REMOVED | info->Action == FILE_ACTION_RENAMED_OLD_NAME | info->Action == FILE_ACTION_RENAMED_NEW_NAME)
			{
				wstring str(info->FileName, info->FileNameLength / sizeof(wchar_t));


				wcout << "Action : " << info->Action <<"|"<<str.c_str() << endl;
				//printf(" %ls", str.c_str());
				info = reinterpret_cast<FILE_NOTIFY_INFORMATION *>(reinterpret_cast<BYTE *>(info) + info->NextEntryOffset);
			}
		} while (info->NextEntryOffset > 0);

		Sleep(1000);
	}

	delete pBuffer;
	CloseHandle(hDir);

	return 0;

	//HANDLE hDir = CreateFile(
	//	L"C:\\",
	//	FILE_LIST_DIRECTORY,
	//	FILE_SHARE_WRITE | FILE_SHARE_READ | FILE_SHARE_DELETE,
	//	NULL,
	//	OPEN_EXISTING,
	//	FILE_FLAG_BACKUP_SEMANTICS,
	//	NULL);

	int nCounter = 0;
	FILE_NOTIFY_INFORMATION strFileNotifyInfo[1024];
	DWORD dwBytesReturned = 0;

	while (TRUE)
	{
		if (ReadDirectoryChangesW(hDir, (LPVOID)&strFileNotifyInfo, sizeof(strFileNotifyInfo), FALSE, FILE_NOTIFY_CHANGE_FILE_NAME, &dwBytesReturned, NULL, NULL) == 0)
		{
			//printf("Reading Directory Change \n");
			Sleep(1000);
		}
		else
		{
			for (int i = 0; i < 10; ++i)
			{
				printf("File Modified: %ls", strFileNotifyInfo[i].FileName);
			}

		
			printf("Loop: %d \n", nCounter++);
		}
	}
}
int mainZ(int argc, char ** argv)
{
	MFT_ENUM_DATA mft_enum_data;
	DWORD bytecount = 1;
	void * buffer;
	USN_RECORD * record = 0;
	USN_RECORD * recordend;
	USN_JOURNAL_DATA * journal;
	DWORDLONG nextid;
	DWORDLONG filecount = 0;
	DWORD starttick, endtick;

	starttick = GetTickCount();

	printf("Allocating memory.\n");

	buffer = VirtualAlloc(NULL, BUFFER_SIZE, MEM_RESERVE | MEM_COMMIT, PAGE_READWRITE);

	if (buffer == NULL)
	{
		printf("VirtualAlloc: %u\n", GetLastError());
		return 0;
	}

	printf("Opening volume.\n");

	drive = CreateFile(L"\\\\?\\c:", GENERIC_READ, FILE_SHARE_DELETE | FILE_SHARE_READ | FILE_SHARE_WRITE, NULL, OPEN_ALWAYS, FILE_FLAG_NO_BUFFERING, NULL);

	if (drive == INVALID_HANDLE_VALUE)
	{
		printf("CreateFile: %u\n", GetLastError());
		return 0;
	}

	printf("Calling FSCTL_QUERY_USN_JOURNAL\n");

	if (!DeviceIoControl(drive, FSCTL_QUERY_USN_JOURNAL, NULL, 0, buffer, BUFFER_SIZE, &bytecount, NULL))
	{
		printf("FSCTL_QUERY_USN_JOURNAL: %u\n", GetLastError());
		return 0;
	}

	journal = (USN_JOURNAL_DATA *)buffer;

	printf("UsnJournalID: %lu\n", journal->UsnJournalID);
	printf("FirstUsn: %lu\n", journal->FirstUsn);
	printf("NextUsn: %lu\n", journal->NextUsn);
	printf("LowestValidUsn: %lu\n", journal->LowestValidUsn);
	printf("MaxUsn: %lu\n", journal->MaxUsn);
	printf("MaximumSize: %lu\n", journal->MaximumSize);
	printf("AllocationDelta: %lu\n", journal->AllocationDelta);

	maxusn = journal->MaxUsn;

	mft_enum_data.StartFileReferenceNumber = 0;
	mft_enum_data.LowUsn = 0;
	mft_enum_data.HighUsn = maxusn;

	for (;;)
	{
		//      printf("=================================================================\n");
		//      printf("Calling FSCTL_ENUM_USN_DATA\n");

		if (!DeviceIoControl(drive, FSCTL_ENUM_USN_DATA, &mft_enum_data, sizeof(mft_enum_data), buffer, BUFFER_SIZE, &bytecount, NULL))
		{
			printf("=================================================================\n");
			printf("FSCTL_ENUM_USN_DATA: %u\n", GetLastError());
			printf("Final ID: %lu\n", nextid);
			printf("File count: %lu\n", filecount);
			endtick = GetTickCount();
			printf("Ticks: %u\n", endtick - starttick);

			printf("=================================================================\n");
			printf("=================================================================\n");
			show_record(record);
			return 0;
		}

		//      printf("Bytes returned: %u\n", bytecount);

		nextid = *((DWORDLONG *)buffer);
		//      printf("Next ID: %lu\n", nextid);

		record = (USN_RECORD *)((USN *)buffer + 1);
		recordend = (USN_RECORD *)(((BYTE *)buffer) + bytecount);

		while (record < recordend)
		{
			filecount++;
			check_record(record);
			record = (USN_RECORD *)(((BYTE *)record) + record->RecordLength);
		}

		mft_enum_data.StartFileReferenceNumber = nextid;
	}
}



void mainA()
{
	HANDLE hVol;
	CHAR Buffer[BUF_LEN];

	USN_JOURNAL_DATA JournalData;
	READ_USN_JOURNAL_DATA ReadData = { 0, 0xFFFFFFFF, FALSE, 0, 0 };
	PUSN_RECORD UsnRecord;

	DWORD dwBytes;
	DWORD dwRetBytes;
	int I;

	hVol = CreateFile(TEXT("\\\\.\\c:"),
		GENERIC_READ | GENERIC_WRITE,
		FILE_SHARE_READ | FILE_SHARE_WRITE,
		NULL,
		OPEN_EXISTING,
		0,
		NULL);

	if (hVol == INVALID_HANDLE_VALUE)
	{
		printf("CreateFile failed (%d)\n", GetLastError());
		return;
	}

	if (!DeviceIoControl(hVol,
		FSCTL_QUERY_USN_JOURNAL,
		NULL,
		0,
		&JournalData,
		sizeof(JournalData),
		&dwBytes,
		NULL))
	{
		printf("Query journal failed (%d)\n", GetLastError());
		return;
	}

	ReadData.UsnJournalID = JournalData.UsnJournalID;

	printf("Journal ID: %I64x\n", JournalData.UsnJournalID);
	printf("FirstUsn: %I64x\n\n", JournalData.FirstUsn);
	DWORD cb = 0;

	do
	{
		memset(Buffer, 0, BUF_LEN);

		if (!DeviceIoControl(hVol,
			FSCTL_READ_USN_JOURNAL,
			&ReadData,
			sizeof(ReadData),
			&Buffer,
			BUF_LEN,
			&dwBytes,
			NULL))
		{
			printf("Read journal failed (%d)\n", GetLastError());
			return;
		}

		dwRetBytes = dwBytes - sizeof(USN);
		cb = dwRetBytes;

		// Find the first record
		UsnRecord = (PUSN_RECORD)(((PUCHAR)Buffer) + sizeof(USN));

		printf("****************************************\n");

		// This loop could go on for a long time, given the current buffer size.
		while (dwRetBytes > 0)
		{
			printf("USN: %I64x\t", UsnRecord->Usn);
			printf("File name: %.*S\t",
				UsnRecord->FileNameLength / 2,
				UsnRecord->FileName);
			printf("Reason: %x\t", UsnRecord->Reason);
			printf("\n");

			dwRetBytes -= UsnRecord->RecordLength;

			// Find the next record
			UsnRecord = (PUSN_RECORD)(((PCHAR)UsnRecord) +
				UsnRecord->RecordLength);
		}
		// Update starting USN for next call
		ReadData.StartUsn = *(USN *)&Buffer;

	} while (!(cb <= 8));

	CloseHandle(hVol);

}


int mainZ()
{
	int nRetCode = 0;

	HMODULE hModule = ::GetModuleHandle(nullptr);

	if (hModule != nullptr)
	{
		// initialize MFC and print and error on failure
		if (!AfxWinInit(hModule, nullptr, ::GetCommandLine(), 0))
		{
			// TODO: change error code to suit your needs
			wprintf(L"Fatal Error: MFC initialization failed\n");
			nRetCode = 1;
		}
		else
		{
			// TODO: code your application's behavior here.
		}
	}
	else
	{
		// TODO: change error code to suit your needs
		wprintf(L"Fatal Error: GetModuleHandle failed\n");
		nRetCode = 1;
	}

	return nRetCode;
}
