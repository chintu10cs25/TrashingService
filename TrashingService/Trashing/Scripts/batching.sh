#!/bin/bash

# Script to batch files and directories from a directory 

# Validate input parameters
if [ -z "$1" ] || [ -z "$2" ] || [ -z "$3" ]; then
  echo "Usage: $0 trashingDirectory batchingDirectory batchsize"
  exit 1
fi

trashingDirectory="$1"
batchingDirectory="$2"
batchsize="$3"
start=1
end="$batchsize"
declare -g fileBatchingCompletion=false
declare -g directoryBatchingCompletion=false

cd "$trashingDirectory"
# Function to create batch files
function create_batch() {
  local start=$1
  local end=$2
  local trashingDirectory=$3
  local batchingDirectory=$4
  local batchingOption=$5

  local batch="$batchingDirectory${batchingOption}_Directory/${start}-${end}.txt"

  # Find files/directories of specified type and write to batch file
  find "$trashingDirectory" -type "$batchingOption" | head -n "$end" | tail -n +"$start" > "$batch"

  # Check if batch file was created and has content
  if [ -f "$batch" ]; then
    if [ -s "$batch" ]; then
      echo "Created batch: $batch"
    else
      rm "$batch"
      if [ "$batchingOption" == "f" ]; then
        fileBatchingCompletion=true
        else
        directoryBatchingCompletion=true
      fi
    fi
  else
    echo "Failed to create batch: $batch"
  fi
}
# Check if directory is empty before creating batches.
while [ -n "$(find "$trashingDirectory" -maxdepth 1 -type d -o -type f )" ]; do
    if [ "$fileBatchingCompletion" = false ]; then
        create_batch "$start" "$end" "$trashingDirectory" "$batchingDirectory" "f"
    fi

    if [ "$directoryBatchingCompletion" = false ];then
        create_batch "$start" "$end" "$trashingDirectory" "$batchingDirectory" "d"
    fi
 
    if [ "$fileBatchingCompletion" = true ] && [ "$directoryBatchingCompletion" = true ]; then
       exit 0
    fi

    start=$((start + batchsize))
    end=$((end + batchsize))
done