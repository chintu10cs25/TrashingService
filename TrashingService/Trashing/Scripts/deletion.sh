#!/bin/bash
# script to process the batch files

# Validate input parameters
if [ -z "$1" ] || [ -z "$2" ]; then
  echo "Usage: $0 batchingDirectory trashingDirectory "
  exit 1
fi

batchingDirectory="$1"
trashingDirectory="$2"
# Function to delete files in batch
function delete_files() 
{
  local batchfile=$1
  Read file contents
  xargs -P 4 rm -v < "$batchfile"
  rm -vf "$batchfile"
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
    echo "Batche processing started"          
    while IFS= read -r -d '' batchfile; do     
      delete_files "$batchfile"
    done < <(find "$batchingDirectory" -maxdepth 1 -type f -print0 | sort -z -r) 
  fi
done