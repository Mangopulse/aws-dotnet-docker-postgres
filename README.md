# aws-dotnet-docker-postgres

this repo is to deploy a sample .net web apps to docker linux on aws

1. we need to create the .net 8 app that has a class library named shared, a web api project that reference the shared project named APi and the solution name is DockerX, also we need another web api (aws-dockerx-api.mangopulse.net) named UI that is a next js app that call the web api with a sub domain (aws-dockerx.mangopulse.net)

2. on the shared project we should have a code that call a postgres db to feth data from table named post (id (guid),title,mediaId,publicid(autoincrement), jsonMeta field for any other data)) also media table (id,aws_s3_path) ,

we should have an api to fetch posts with their media (id,title,mediaurl) and an api to create a new post and upload its photo and save it to s3 bucket in aws


we should have a docker container for api container for Ui, container for postgres db 

make sure to use docker compose

also make sure the code to follow, OOP, SOLID, DRY, SRP, SOP, TDD with a project to run tests

THe ui project should display the posts cards with photos and a button to upload a new post and its photo

make sure the connection string to be in a file name config.json on the root of the repository

make sure also to create github actions for deployment and a terra form file for aws machine creation 
tests should be integrtion tests create an empty db create tables schema postgress on the docker and try to create a post and a test to return posts


LEts make a web project named front-api (fetch posts), front(nextjs to show posts cards),admin-api(should be also authenticathed jwt token) ,admin(next js to create post and upload media  and call admin-api, authenticathed  and login page)



Also we need to have an upload project that has and upload controller named storeController 

that recive an photo upload and save it in s3 bucket or azure blob or local file system

Also we need a media project that use imageflow to server images and crop images