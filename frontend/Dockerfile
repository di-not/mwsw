FROM node:20-alpine
WORKDIR /app

RUN npm install -g serve@14
COPY . .

CMD ["sh", "-c", "serve -l ${PORT:-3000} ."]
