# 使用 Node.js 作為建置映像
FROM node:20-alpine AS build
WORKDIR /app

# 複製 package.json 並安裝相依套件
COPY src/WebCMS.Web/package*.json ./
RUN npm ci

# 複製原始碼並建置
COPY src/WebCMS.Web/ .
RUN npm run build -- --configuration production

# 使用 Nginx 作為執行映像
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# 移除預設的 Nginx 靜態檔案
RUN rm -rf ./*

# 複製建置的檔案
COPY --from=build /app/dist/webcms-web/browser .

# 複製 Nginx 設定
COPY src/WebCMS.Web/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
