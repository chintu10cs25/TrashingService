#!/bin/bash
# script to process the batch files
# Validate input parameters
if [ -z "$1" ] || [ -z "$2" ]; then
  echo "Usage: $0 batchingDirectory trashingDirectory "
  exit 1
fi

trashingDirectory="$1"
batchingDirectory="$2"

# set working directory
cd "$trashingDirectory"
# Function to delete files in batch
function delete_files() 
{
  local batchingDirectory="$1"
  local deletionOption="$2"
  local f_Directory="$batchingDirectory${deletionOption}_Directory/"
  while IFS= read -r -d '' batchfile; do     
    # Read file contents and remove files
    # xargs -P 4 rmdir -vp < <(tac "$batchfile")
    xargs -P 4 rm -v < <(tac "$batchfile")
    rm -vf "$batchfile"
  done < <(find "$f_Directory" -maxdepth 1 -type f -print0 | sort -z -r) 
 
}

# Function to delete empty directories in batch
function delete_Directories()
{
  local batchingDirectory="$1"
  local deletionOption="$2"
  local d_Directory="$batchingDirectory${deletionOption}_Directory/"
  while IFS= read -r -d '' batchfile; do     
    # Read file contents and remove direcories
    xargs -P 4 rmdir -vp --ignore-fail-on-non-empty < <(tac "$batchfile")
    rm -vf "$batchfile"
  done < <(find "$d_Directory" -maxdepth 1 -type f -print0 | sort -z -r) 
}
while [  -n "$(find "$trashingDirectory" -maxdepth 1 -type d -o -type f )" ]; do
  # Wait and watch for changes to batching directory if it is empty other wise start processing
  if [ -z "$(find "$batchingDirectory" -type f)" ]; then
    echo "Batching directory $batchingDirectory is empty Waiting for batches"
    if batchfile=$(inotifywait -q -e create --format '%w%f' "$batchingDirectory"); then
      # Wait and watch for the file to be closed using inotifywait
      echo "Waiting for batch file $batchfile to be closed..."
      while inotifywait -q -e close_write "$batchfile"; do
        echo "Batch file $batchfile is closed"
        sleep 60              
        break;
      done
    fi
  else
    # set working directory to  echo "Batch processing started"
    echo "Batch processing started"
    delete_files "$batchingDirectory" "f"
    delete_Directories "$batchingDirectory" "d"
  fi
done