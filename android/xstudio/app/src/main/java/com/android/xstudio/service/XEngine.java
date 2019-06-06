package com.android.xstudio.service;

import android.app.Application;
import android.content.Context;
import android.provider.Settings;
import android.util.Base64;
import android.util.Log;

import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONArray;
import com.android.xstudio.Utils.Console;
import com.android.xstudio.model.Response;

import org.joor.Reflect;
import org.mozilla.javascript.Function;
import org.mozilla.javascript.Scriptable;
import org.mozilla.javascript.ScriptableObject;

import java.lang.reflect.Array;

import de.robv.android.xposed.callbacks.XC_LoadPackage;

/**
 * Created by Mi on 2019/4/14.
 */

public class XEngine {
    private static String TAG = "XSTUDIO";

    public static Response request(Context ctx, XC_LoadPackage.LoadPackageParam lpparam , String source, Object args) {

        Response res = new Response();
        org.mozilla.javascript.Context rhino = org.mozilla.javascript.Context.enter();
        rhino.setOptimizationLevel(-1);
        try {
            Scriptable scope = rhino.initStandardObjects();
            ScriptableObject.putProperty(scope, "console", new Console());

            rhino.evaluateString(scope, source, null, 1, null);
            Function function = (Function) scope.get("main", scope);

            if (function == Scriptable.NOT_FOUND) {
                res.code = -1;
                res.data = "NOT_FOUND";
            } else {
                res.code = 0;
                res.data = org.mozilla.javascript.Context.jsToJava(function.call(rhino, scope, scope, new Object[]{lpparam, ctx, args}), Object.class);
            }
        } catch (Exception e) {
            //Log.e(TAG,  e.toString());
            //Log.e(TAG,  Log.getStackTraceString(new Throwable()));
            res.code = -1;
            res.data = e.toString();
        } finally {
            org.mozilla.javascript.Context.exit();
        }
        return res;
    }
}
