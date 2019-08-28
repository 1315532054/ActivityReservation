# 预约 API 云函数

## Intro

微信小程序的域名需要备案，但是没有大陆的服务器，而且觉得备案有些繁琐，起初做的小程序都有点想要放弃了，后来了解到腾讯云的云函数，于是利用腾讯云的云函数实现了一个简单的 API 网关，通过云函数来调用真正的 API 地址，借此来绕过域名备案的问题

## 使用方式

修改 `index.ts` 文件中的要转发的地址

在该目录下执行 `tsc`，生成编译后的 js

到 dist 目录下执行 `npm install`，安装依赖，目前用到的只有一个 `got`，如果用到了别的请在 `package.json` 文件里添加，或者执行 `npm install <package-name> --save`

之后打包 dist 目录下的内容到 zip，然后上传到腾讯云的控制台即可

![upload code](./images/upload.png)

需要注意，压缩包不能包含 dist 目录，打开压缩包之后就是代码

dist.zip

- -- node_modules
- -- httpRequester.js
- -- index.js
- -- packages.json

## 实现原理

请求转发，实现一个简单的 API 网关

## 实现过程中遇到的问题

> `unable to verify the first certificate`

这个是 https 请求证书验证的问题，参考 stackoverflow <https://stackoverflow.com/questions/31673587/error-unable-to-verify-the-first-certificate-in-nodejs/32440021>

通过设置了一个环境变量 `NODE_TLS_REJECT_UNAUTHORIZED=0` 来解决了

> 访问 api 404

访问之后，通过看日志，输出请求的地址发现，request 的 path 是带函数名称的，所以将函数名去掉就可以了

``` typescript
if ((<string>event.path).startsWith('/reservationWxAppGateway')) {
    event.path = (<string>event.path).replace('/reservationWxAppGateway', '');
}
```

后来发现在 `event` 的参数里有个 `event.requestContext.path` 来表示云函数的 path，把这个 path 去掉就是真正请求的路径

``` typescript
if((<string>event.path).startsWith(`${event.requestContext.path}`)){
    event.path = (<string>event.path).replace(`${event.requestContext.path}`, '');
}
```

> 请求头的转发

请求头转发的时候， `host` 请求头不能传，我在传递 headers 的时候将 `host` 请求头设置为 `undefined`

``` typescript
headers["host"]= undefined;
```
