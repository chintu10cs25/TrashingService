#!/bin/bash

# Set the maximum system load that we want to allow
MAX_LOAD=10.0

# Set the path to the directory that contains the files and directories to delete
#DIRECTORY="/path/to/directory"
DIRECTORY="/home/chintu/Trash"

# Set the batch size for deletion (adjust as needed)
BATCH_SIZE=1000

# Set the maximum number of parallel processes to run (adjust as needed)
MAX_PROCESSES=4

# Set the minimum free disk space to maintain (adjust as needed) 
MIN_FREE_SPACE=1000000000  # 1GB

# Loop until all files and directories have been deleted
while true; do

  # Get the current system load
  SYSTEM_LOAD=$(uptime | awk -F 'load average: ' '{print $2}' | awk -F, '{print $1}')

  # Check if the system load is below the maximum load threshold
  if (( $(echo "$SYSTEM_LOAD < $MAX_LOAD" |bc -l) )); then

    # Get the total number of files and directories in the directory
    TOTAL=$(find "$DIRECTORY" -type f -o -type d | wc -l)

    # Check if there are any files or directories left to delete
    if [ "$TOTAL" -eq 0 ]; then
      echo "All files and directories deleted." 
      exit 0
    fi

    # Calculate the number of batches to run
    BATCHES=$((($TOTAL + $BATCH_SIZE - 1) / $BATCH_SIZE))

    # Delete files and directories in batches
    for ((i=0; i<$BATCHES; i++)); do
      find "$DIRECTORY" -type f -o -type d | head -n "$BATCH_SIZE" | nice -n 19 xargs -r -P $MAX_PROCESSES rm -rf
    done

    # Get the current free disk space
    FREE_SPACE=$(df -P "$DIRECTORY" | awk '{print $4}' | tail -n 1)

    # Check if there is enough free disk space
    if [ "$FREE_SPACE" -lt "$MIN_FREE_SPACE" ]; then
      echo "Low disk space. Stopping deletion process."
      exit 1
    fi

  else
    echo "System load too high. Waiting..."
  fi

  # Wait for 1 second before checking the system load again
  sleep 1

done

