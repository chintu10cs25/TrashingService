for file in /home/chintu/BatchesIn10K/*
do
    echo "$file"
	xargs -P 4 rm -v < "$file"
	rm "$file"
done
