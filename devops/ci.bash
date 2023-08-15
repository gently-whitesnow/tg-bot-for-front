#!/bin/bash

git pull

sudo rm -rf /etc/nginx/*
sudo mkdir -p /etc/nginx
sudo cp -rf ./nginx.conf /etc/nginx/nginx.conf

docker-compose up --force-recreate -d