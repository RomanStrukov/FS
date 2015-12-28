#define FUSE_USE_VERSION 26

#include <errno.h>
#include <string.h>
#include <stdlib.h>
#include <stdio.h>
#include <fuse.h>
#include <fcntl.h>

#define FUSE_SRC_FILE "/home/fileSystem/fs"
#define FILENAME_MAX_LENGTH 100
#define BLOCK_NUMBER 2048
#define BLOCK_SIZE 2048
#define FILE_NUMBER 64

typedef enum{ false, true} bool;

struct {
	char name[FILENAME_MAX_LENGTH]; //name
	int startBlock;			//number of startBlock block
	int size;			//size
	bool isDirectory;			//is directory (or file)
	bool isNotFree;			//is free
} fileMeta;

	// block[i] >= 0   next block's index, 
	//          == -1  end,
	//          == -2  empty
struct{
	fileMeta meta[FILE_NUMBER];

	
	int block[BLOCK_NUMBER];
} fileSystem;

fileSystem fs; //global FS

void initFS()
{
	int i = 0;
	while (i < sizeof(fs.meta / fs.meta[0])) 
	{
		memset(fs.meta[i].name, 0, FILENAME_MAX_LENGTH);
		fs.meta[i].isDirectory = false;
		fs.meta[i].isEmpty = true;
		fs.meta[i].blockStart = -1;
		fs.meta[i].blockSize = 0;
		i++;
	}
	i = 0;
	for(i = 0; i < BLOCK_NUMBER - 1; i++)
	{
	   fs.block[i+1] = -2; //set "empty"
	}
}

void restoreFS()
{
  FILE *FILE = fopen(FUSE_SRC_FILE, "r");
	fread(fs.meta, sizeof(fmeta_t), FILE_NUMBER, FILE);
	fread(fs.block, sizeof(int), BLOCK_NUMBER, FILE);	
	int i;
	for(i = 0; i < BLOCK_NUMBER; i++)
	fs.block[i] = fs.block[i] == 0 ? -2 : fs.block[i];
	fclose(f);
}

fileMeta *getMetaAtIndex(int metaIndex) 
{
	return &fs.meta[metaIndex];
}

int writeBlocks(fileMeta *currentMeta) 
{
	FILE *f = fopen(FILE_PATH, "r+");
	int i = currentMeta->startBlock;
	do {
		fseek(f, sizeof(fileMeta) * FILE_NUMBER + sizeof(int) * i, SEEK_SET);
		fwrite(&fs.block[i], sizeof(int), 1, f);
		i = fs.block[i];
	}
	while (i != -1);
	fclose(f);
	return 0;
}

int writeMeta(int metaIndex) 
{
	FILE *f = fopen(FILE_PATH, "r+");
	fseek(f, metaIndex * sizeof(fileMeta), SEEK_SET);

	fileMeta* currentMeta = getMetaAtIndex(metaIndex);
	fwrite(currentMeta, sizeof(fileMeta), 1, f);

	fclose(f);
	return 0;
}

int writeData(fileMeta *currentMeta, const char *data, int size, int offset) 
{
	if (size == 0) 
		return 0;

	FILE *f = fopen(FILE_PATH, "r+");

	int i = currentMeta->startBlock;
	int j = offset / BLOCK_SIZE;
	while (j-- > 0) 
		i = fs.block[i];
	
	int left = size;
	int count = 0;
	int remain = offset % BLOCK_SIZE;

	int skip = sizeof(fileMeta) * FILE_NUMBER + sizeof(int) * BLOCK_NUMBER;

	while (left > 0) 
	{ 		
		if (remain + left > BLOCK_SIZE)
			k = BLOCK_SIZE - remain;
		else
			k = left;

		fseek(f, skip + i * BLOCK_SIZE + remain, SEEK_SET);
		fwrite(data + size - left, 1, k, f);

		if (remain + k == BLOCK_SIZE) 
		{
			if (fs.block[i] >= 0) i = fs.block[i];
			else 
			{
				if (findEmptyBlock() == -1)
				 return -1;
				fs.block[i] = findEmptyBlock();
				fs.block[findEmptyBlock()] = -1;
				i = findEmptyBlock();
			}
		}

		left = left - k;
		remain = 0;
	}
	currentMeta->size = size + offset;

	writeBlocks(currentMeta);
	fclose(f);
	return size;	
}

int addFile(char* fileName, int size, bool isDirectory) 
{
	fileMeta *meta = NULL;

	if (findEmptyMeta() == -1) 
		return -1;
	meta = getMetaAtIndex(k);
	if (findEmptyBlock() == -1) 
		return -1;

	strcpy(meta->fileName, fileName);

	meta->startBlock = findEmptyBlock();
	meta->size = size;
	meta->isDirectory = isDirectory;
	meta->isNotFree = false;

	int first = findEmptyBlock()
	fs.block[first] = -1;
	writeMeta(k);
	writeBlocks(meta);

	return k;
}

int readData(fileMeta *currentMeta, char **data) 
{

	if (currentMeta == NULL) 
		return -1;

	FILE *f = fopen(FILE_PATH, "r");
	char *buffer = (char *)malloc(currentMeta->size);
	int i = currentMeta->startBlock;
	int k = 0;
	int count;
	int skip = sizeof(fileMeta) * FILE_NUMBER + sizeof(int) * BLOCK_NUMBER;

	while (i != -1) 
	{
		int left = currentMeta->size - k * BLOCK_SIZE;

		if (left >= BLOCK_SIZE) 
			count = BLOCK_SIZE;
		else 
			count = left;
		fseek(f, skip + i * BLOCK_SIZE, SEEK_SET);
		fread(buffer + k * BLOCK_SIZE, 1, count, f);

		i = fs.block[i];
		k++;
	}
	fclose(f);
	*data = buffer;
	return currentMeta->size;
}

int createFileOrDirectory(const char* path, bool isDirectory) 
{
	fileMeta *meta;
	char *directory, *fileName, *data, *extData;

	fileName = strrchr(path, '/');

	if (fileName == NULL) 
	{
		strcpy(fileName, path);
		directory = (char*)malloc(2);
		strcpy(directory, "/\0");
	} 
	else 
	{
		fileName++;
		directory = (char *)malloc(strlen(path) - strlen(fileName) + 1);
		strncpy(directory, path, strlen(path) - strlen(fileName));
		directory[strlen(path) - strlen(fileName)] = '\0';
	}
	printf("Directory: %s fileName: %s\n", directory, fileName);
	extData = (char*)malloc(readData(meta, &data) + sizeof(int));
	memcpy(extData, data, size);

	int n = size/sizeof(int)
	((int*)extData)[n] = addFile(fileName, 0, isDirectory);

	int resSize = size + sizeof(int)
	writeData(meta, extData, resSize, 0);
	meta->size = resSize;	
	writeMeta(getMeta(directory, &meta));

	free(extData);
	free(directory);
	return 0;
}

int removeFileOrDirectory(const char *path)
{
	int res = remove(path);
	return res != 0 ? -1 : 0;
}

int remove(const char* path)
 {
	fileMeta *fileMeta, *dMeta;
	char *data, *extData;
	char *dir = getDirPath(path);
	printf("Removed: Directory = %s\t Path = %s\n", dir, path);

	int dMetaNum = getMeta(dir, &dMeta);
	int fMetaNum = getMeta(path, &fileMeta);
	int size = readData(dMeta, &data);
	extData = (char *)malloc(size - sizeof(int));

	int i = 0, j = 0;
	while (i < size / sizeof(int)) 
	{
		//magic...
		((int *)extData)[j++] = ((int *)data)[i] != fMetaNum ? ((int *)data)[i] : ((int *)extData)[j++];
		i++; 
	}

	writeData(dMeta, extData, size, 0);
	dMeta->size = size - sizeof(int);
	writeMeta(dMetaNum);

	free(data);
	free(dir);
	return 0;
}

int getMeta(const char *path, fileMeta **meta) 
{
	char *fpath = (char*)malloc(strlen(path));
	strcpy(fpath, path);
	printf("%s\n", fpath);

	if (fpath && strcmp("/", fpath) == 0) { 
		//rootDir
		*meta = getMetaAtIndex(0); 	
		return 0;
	}

	fileMeta *m = NULL;
	char *p;

	p = fpath;

	if (*p++ == '/')
		m = getMetaAtIndex(0);
	else return -1;

	char *data, *s;
	char name[FILENAME_MAX_LENGTH];
	memset(name, '\0', FILENAME_MAX_LENGTH);

	int k = -1, size;
	
	while (p - fpath < strlen(fpath)) {
		if (m->size == 0)
			return -1;
		size = readData(m, &data);
		s = p;
		p = strchr(p, '/');
		if (p != NULL) {
			p = p + 1;
			strncpy(name, s, p - s - 1);			
		}
		else {
			strncpy(name, s, fpath + strlen(fpath) - s);
			p = fpath + strlen(fpath);		
		}
		k = getFileMetaNumber(data, name, size);
		if (k == -1) return -1;
		m = getMetaAtIndex(k);
		memset(name, '\0', FILENAME_MAX_LENGTH);
		free(data);
	}

	*meta = m;
	return k;
}

int openFile (const char *path,)
{	
	fileMeta *meta;
	int metaIndex = getMeta(path, &meta);
	return metaIndex == -1 ? -1 : 0;
}

int read (const char *path, char *buf, size_t size, off_t offset)
{
	fileMeta *currentMeta;

	if (getMeta(path, &currentMeta) == -1) 
		return -ENOENT;

	char *data;
	int result = readFile(currentMeta, &data, size, offset);
	if (result == -1) 
		return -ENOENT;
	memcpy(buf, data, result);
    	return result;
}

int createFile(const char *path)
{
	return createFileOrDirectory(path, false) != 0 ? -1 : 0;
}

int mkdir(const char *path)
{
	return createFileOrDirectory(path, true) != 0 ? -1 : 0;
}

int readdir (Meta *meta, char * data)
{

}

static void *fs_init(struct fuse_conn_info *fi) 
{
	restoreFS();	
}
static int fs_getattr(const char* path, struct stat *stbuf) 
{
	int res = 0;

	fileMeta *meta;
	if (getMeta(path, &meta) == -1)
		res = -ENOENT;

	memset(stbuf, 0, sizeof(struct stat));
    	if(meta->isDirectory) {
		stbuf->st_mode = S_IFDIR | 0755;
		stbuf->st_nlink = 2;
    	}
    	else {
        	stbuf->st_mode = S_IFREG | 0444;
        	stbuf->st_nlink = 1;
        	stbuf->st_size = meta->size;
    	}
	stbuf->st_mode = stbuf->st_mode | 0777;
    	return res;
}
static int fs_open(const char *path, struct fuse_file_info *fi)
{
	return openFile(path) == -1 ? -ENOENT : 0;
}
static int fs_read(const char *path, char *buf, size_t size, off_t offset,
		      struct fuse_file_info *fi) 
{
	return read(path, buf, size, offset);
}
static int fs_create(const char *path, mode_t mode, struct fuse_file_info *finfo) 
{
	return createFile(path)
}
static int fs_mkdir(const char *path, mode_t mode)
{
	return mkdir(path);
}
static int fs_rmdir(const char *path)
 {
	return removeFileOrDirectory(path);
}

static int fs_unlink(const char *path) 
{
	return removeFileOrDirectory(path);
}
static int fs_readdir(const char *path, void *buf, fuse_fill_dir_t filler,
			 off_t offset, struct fuse_file_info *fi)
{
	
}

//associating fuse functions with current realisations
static struct fuse_operations fuse_oper = 
{
        .getattr        = fs_getattr,
        .create         = fs_create,
        .unlink			= fs_unlink,
        .open           = fs_open,
        .read           = fs_read,
	.mkdir          = fs_mkdir
	.rmdir			= fs_rmdir,
	.readdir        = fs_readdir,
	.init  			= fs_init
};

int main(int argc, char *argv[])
{
	return fuse_main(argc, argv, &fs_oper, NULL);
}
