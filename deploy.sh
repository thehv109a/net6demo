git pull; docker build -t netcore6demo:latest -f Dockerfile .;
docker rm -f netcore6demo; docker run --name netcore6demo -p 180:80 -d netcore6demo:latest
