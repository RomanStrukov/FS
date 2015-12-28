#define FUSE_USE_VERSION 26

#include <config.h>
#include <fuse.h>
#include <stdio.h>
#include <string.h>
#include <errno.h>
#include <fcntl.h>

#define FUSE_SRC_FILE "/home/fileSystem/fs/"
#define FILENAME_MAX_LENGTH 100
#define BLOCK_NUMBER 2048
#define BLOCK_SIZE 1024
#define FILE_NUMBER 64
typedef enum{ false, true} bool;



typedef struct Meta {
	char name[MAX_FILENAME_LENGTH]; 
	bool isDirectory;			
	bool isEmpty;
	int blockStart;			
	int blockSize;	
} Meta;

typedef struct FileSystem {
	Meta meta[FILE_NUMBER];
	int block[BLOCK_NUMBER];
	// -1: end of file 
	// -2: is empty 
	// >= 0: next block's number
} FileSystem;

FileSystem fs; //global FS

#region myMethods
void initFS()
{
	int i = 0;
	while (i < sizeof(fs.meta / fs.meta[0])) 
	{
		memset(fs.meta[i].name, 0, MAX_FILENAME_LENGTH);
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
	int i = 0;
	for(i = 0; i < BLOCK_NUMBER; i++)
	fs.block[i] = fs.block[i] == 0 ? -2 : fs.block[i];
	fclose(f);
}

int getattr (const char* path, struct stat *stbuf)
{

}
int open (const char *path,)
{

}
int read (const char *path, Meta *meta)
{

}
int write (int fd, const char *buf, size_t size, off_t offset)
{

}
int mkdir(const char *path, mode_t mode)
{

}
int readdir (Meta *meta, char * data)
{

}
#endregion


#region Fuse methods
static int fs_getattr(const char* path, struct stat *stbuf) 
{
	int res;

	res = getattr(path, stbuf);
	if (res == -1)
		return -errno;

	return 0;
}
static int fs_open(const char *path, struct fuse_file_info *fi)
{
	int res;

	res = open(path, fi->flags);
	if (res == -1)
		return -errno;

	close(res);
	return 0;
}
static int fs_read(const char *path, char *buf, size_t size, off_t offset,
		      struct fuse_file_info *fi) 
{
	Meta *meta = getMeta(path); //release!
	size_t len = (size_t)read(path, meta);
		
	if (offset < len) {
		if (offset + size > len)
			size = len - offset;
		memcpy(buf, hello_str + offset, size);
	} else
		size = 0;

	return size;
}
static int fs_write(const char *path, const char *buf, size_t size,
		     off_t offset, struct fuse_file_info *fi)
{
	int fd;
	int res;

	(void) fi;
	fd = open(path, O_WRONLY);
	if (fd == -1)
		return -errno;

	res = write(fd, buf, size, offset);
	if (res == -1)
		res = -errno;

	close(fd);
	return res;
}
static int fs_mkdir(const char *path, mode_t mode)
{
	int res;

	res = mkdir(path, mode);
	if (res == -1)
		return -errno;

	return 0;
}
static int fs_readdir(const char *path, void *buf, fuse_fill_dir_t filler,
			 off_t offset, struct fuse_file_info *fi)
{
	Meta *meta;
	char *data;
	readdir(meta, &data);
	while (i < sizeof(data)/sizeof(int)) {
		filler(buf, fs.meta[((int*)data)[i]].name, NULL, 0);
	}
    	return 0;
}
#endregion

//associating fuse functions with current realisations
static struct fuse_operations fuse_oper = 
{
        .getattr        = fs_getattr,
        .open           = fs_open,
        .read           = fs_read,
	.write          = fs_write,
	.mkdir          = fs_mkdir
	.readdir        = fs_readdir,
};

int main(int argc, char *argv[])
{
	return fuse_main(argc, argv, &fs_oper, NULL);
}
