package com.android.xstudio.model;

import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.annotation.JSONField;
import com.alibaba.fastjson.serializer.SerializeConfig;

/**
 * Created by Mi on 2019/4/12.
 */

public class Response {
    @JSONField(name = "code")
    public int code;
    @JSONField(name = "data")
    public Object data;

    @Override
    public String toString() {
        return JSON.toJSONString(this);
    }
}
