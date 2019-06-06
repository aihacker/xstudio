#### 软件安装
---
1.安装XP框架并且激活（具体方法自行百度）

![image](http://pqdatnbng.bkt.clouddn.com/TIM%E6%88%AA%E5%9B%BE20190422224753.png)

2.安装XSTUDIO模块并且激活

![image](http://pqdatnbng.bkt.clouddn.com/TIM%E6%88%AA%E5%9B%BE20190422224812.png)

3.运行REDIS服务

![image](http://pqdatnbng.bkt.clouddn.com/redis.png)
![image](http://pqdatnbng.bkt.clouddn.com/redis2.png)


#### 配置说明

1.安卓端配置

![image](http://pqdatnbng.bkt.clouddn.com/apk.png)

2.电脑端配置

![image](http://pqdatnbng.bkt.clouddn.com/pc.png)

3.REDIS配置

![image](http://pqdatnbng.bkt.clouddn.com/redis3.png)
---

#### 架构说明

```
graph LR
电脑-->REDIS
REDIS-->安卓
安卓-->REDIS
REDIS-->电脑
```


- 为什么要多一个REDIS作为消息中间件

1. REDIS高性能，搞并发
2. 安卓端很难暴露外网接口，很难组成集群网络
3. 一个REDIS, 一个电脑端口，连接多个安卓客户端，实现高并发请求，组成集群任务处理后台，时速上百万不成问题。
4. 容错功能更好，当安卓端口出现各种问题（假死，掉线）后，还有其他安卓端进行数据处理

- 为什么要用js作为脚本语言，而不用其他方案

1.安卓端使用 rhino 作为js解释器，火狐出品。更多相关细节请自行百度；
2.为了更加灵活的调用各种接口。
3.免编译，免重启，快速调试更新。

#### 图标说明

![image](http://pqdatnbng.bkt.clouddn.com/file.png)

新建/保存/删除/设置/美化/调用/HOOK/清理

#### CALL代码

```
//别名
var ref = org.joor.Reflect //joor 反射库
var log = android.util.Log //日志功能

/*
*main：  执行调用入口
*lpparm：XPOSED handleLoadPackage 传入参数
*ctx：   安卓应用上下文
*param： 附加参数
*/
var main = function(lpparm, ctx, param) {
	console.log(lpparm.packageName)
    console.log(lpparm.classLoader)
    return "hello word"
}
```

#### HOOK代码

```
//find 查找需要HOOK的方法
var find = function(lpparm, ctx, param) {
    //XC_LoadPackage.LoadPackageParam lpparm
    //llppm.lpparam.packageName
    //lpparam.classLoader
    //lpparam.processName
    //Content cxt app的上下文；
    //param null
}


//调用前
//before_func = protected void beforeHookedMethod(MethodHookParam param)
//当XPOSED框架触发该方法是会调用 before_func 方法
var before_func = function(param) {
    //param.args;//传入参数
    //param.thisObject;//对象本身
}

//调用后
//after_func =  protected void afterHookedMethod(MethodHookParam param) 
//当XPOSED框架触发该方法是会调用 after_func 方法
var after_func = function(param) {
    //param.args;//传入参数
    //param.thisObject;//对象本身
    //param.getResult();\\获取返回值
    //param.setResult();\\设置返回值
}
```

#### 生成代码

![image](http://pqdatnbng.bkt.clouddn.com/tool.png)

生成的代码，HOOK功能是可以直接使用的，调用功能仅当参考，因为调用本来就是一件很灵活的事情，需要寻找合适的调用入口，那些功能可以有本地实现，那些功能由调用实现都是一件很值得考虑的事情。最重要的还是要找到合适的入口（静态方法）；


#### 使用的库
1.XposedInstaller [github](https://github.com/rovo89/XposedInstaller)

2.fastjson        [github](https://github.com/alibaba/fastjson)

3.jOOR            [github](https://github.com/jOOQ/jOOR)

4.rhino           [github](https://github.com/mozilla/rhino)

5.jedis           [github](https://github.com/xetorthio/jedis)

6.okhttp          [github](https://github.com/square/okhttp)


- 为什么返回的数据是byte[]数组，结果却是base64。归功于 fastjson 放回结果用 fastjson 包装了一下，同时也要注意，如果不想被包装，请用toJsong();
- jOOR 是一个简单易用的java反射库。
- rhino js脚本解释器，提供了 jsva js 混合开发的功能，建议详细了解
- okhttp 暂时没有用到，当时为了更加灵活还是引用一下
- Xposed 这个也要详细了解 ==find=handleLoadPackage==/==after_func=afterHookedMethod==/==before_func=beforeHookedMethod==

> 以上教程百度一大把，不懂的地方多多百度


#### 其他说明
1.碰到不明吧的东西怎么办？

多调试，调试不要钱，比如 lpparam 我不知道是什么东西，可以来一个
```
console.log(lpparm.getClass().toString())
```
让后在视图里面搜索一下，让后再百度一下。

2.代码提示不够完善？

碰到新的系统类多用一下视图里面的搜索功能，如果是android或java开头的包名，系统会记下这些方法，在你下次输入的时候就可以愉快的使用提示了；

3.studio.db 是什么文件？

studio.db 是一个数据库文件。用于保存文件 关键字等信息;

4.文件选项卡里面的的==钩选框==有什么用？

如果是HOOK文件，或者你要使用这个HOOK文件，就勾选一下吧。如果要清楚HOOK可以手动删除安卓SD卡里面的 HOOK.json, 也可以全部都不勾选。发布一次；

5.免重启是指？

免重启系统，更新Hook文件后，重启apk还是必须的，因为HOOK的注入只有在APK加载的时候加载；

6.怎么查看错误的日志？

可以用logcat工具 过滤TAG XSTUDIO