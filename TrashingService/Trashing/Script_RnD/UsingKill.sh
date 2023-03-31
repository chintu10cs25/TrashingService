
nice -n 19 ionice -c 3 rsync -a --delete /path/to/source/ /path/to/destination/ &
RSYNC_PID=$!

while true; do
    LOAD=$(uptime | awk '{print $(NF-2)}' | sed 's/,//g')
    if [[ $(echo "$LOAD > 5" | bc -l) -eq 1 ]]; then
        echo "System load is too high, pausing rsync"
        kill -SIGSTOP $RSYNC_PID
    else
        echo "System load is low, resuming rsync"
        kill -SIGCONT $RSYNC_PID
    fi
    sleep 5
done
