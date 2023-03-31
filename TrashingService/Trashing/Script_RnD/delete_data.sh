#!/bin/bash

# Set the maximum system load threshold
max_load=1.0

# Get the current system load average
load=$(uptime | awk '{print $10}' | sed 's/,//')

# Check if the load is below the threshold
if (( $(echo "$load < $max_load" | bc -l) )); then
    # Synchronize an empty directory with the basePath directory using rsync
    rsync -a --delete /path/to/empty/ "$basePath/"
fi
