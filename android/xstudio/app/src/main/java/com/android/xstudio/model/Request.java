package com.android.xstudio.model;

import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.annotation.JSONField;

/**
 * Created by Mi on 2019/4/12.
 */


public class Request {
    @JSONField(name = "uuid")
    public String uuid;
    @JSONField(name = "source")
    public String source;
    @JSONField(name = "args")
    public Object args;

    public static Request fromString(String s){
        Request req = JSON.parseObject(s, Request.class);
        return req;
    }
}


