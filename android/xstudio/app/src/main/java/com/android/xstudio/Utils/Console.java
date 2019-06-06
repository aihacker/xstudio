package com.android.xstudio.Utils;

import android.text.TextUtils;

import com.alibaba.fastjson.JSON;
import com.android.xstudio.R;
import com.android.xstudio.service.XService;

import org.joor.Reflect;

import java.lang.reflect.Constructor;
import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

/**
 * Created by root on 2019/4/18.
 */

public class Console {
    public static void log(String s) {
        if (! TextUtils.isEmpty(s)){
            XService.getInstarnce().publish(s);
        }
    }

    public static void log(Object o) {
        if (o != null){
            XService.getInstarnce().publish(JSON.toJSONString(o));
        }
    }
}
