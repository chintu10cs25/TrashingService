batchsize=10000
start=1
end="$batchsize"
while [ -z "$(find {basePath} -maxdepth 0 -empty)" ]; do
    sleep 1
done
while [ -n "$(find {basePath} -maxdepth 0 -type f)" ]; do
    find {basePath} -type f | head -n "$end" | tail -n +"$start" > {fileName}
    start=$((start + batchsize))
    end=$((end + batchsize))
done

