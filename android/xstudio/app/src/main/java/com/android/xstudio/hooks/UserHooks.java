package com.android.xstudio.hooks;

import android.content.Context;
import android.text.TextUtils;
import android.util.Log;

import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONArray;
import com.android.xstudio.Utils.Console;
import com.android.xstudio.Utils.FileUtils;

import org.joor.Reflect;
import org.mozilla.javascript.Function;
import org.mozilla.javascript.Scriptable;
import org.mozilla.javascript.ScriptableObject;

import java.lang.reflect.Member;

import de.robv.android.xposed.XC_MethodHook;
import de.robv.android.xposed.XposedBridge;
import de.robv.android.xposed.callbacks.XC_LoadPackage;

/**
 * Created by Mi on 2019/4/13.
 */

public class UserHooks extends XC_MethodHook {
    private static String TAG = "XSTUDIO";


    public static void initAllHooks(Context ctx, final XC_LoadPackage.LoadPackageParam lpparam) {
        String json = FileUtils.read("hooks.json");
        if (json == null || json.length() == 0){

            return;
        }

        try {
            JSONArray list = JSON.parseArray(json);
            for (int i = 0; i < list.size(); i++) {
                String source = list.getString(i);
                if (TextUtils.isEmpty(source))
                    continue;

                org.mozilla.javascript.Context rhino = org.mozilla.javascript.Context.enter();
                rhino.setOptimizationLevel(-1);
                try {

                    Scriptable scope = rhino.initStandardObjects();
                    ScriptableObject.putProperty(scope, "console", new Console());

                    rhino.evaluateString(scope, source ,null, 1, null);
                    Function function = (Function) scope.get("find",scope);
                    if (function == Scriptable.NOT_FOUND){
                        continue;
                    }

                    Object result = org.mozilla.javascript.Context.jsToJava(function.call(rhino, scope, scope, new Object[]{lpparam, ctx, null}), Member.class);

                    if (result != null && result instanceof Member){
                        Log.i(TAG, "hook: " + ((Member) result).getName());
                        XposedBridge.hookMethod((Member)result, new MethodHook(source));
                    }

                }catch (Exception e) {
                    Log.e(TAG,  e.toString());
                    Log.e(TAG,  Log.getStackTraceString(new Throwable()));
                }finally {
                    org.mozilla.javascript.Context.exit();
                }
            }
        } catch (Exception e) {
            Log.e(TAG,  e.toString());
            Log.e(TAG,  Log.getStackTraceString(new Throwable()));
        }
    }
    

    static class MethodHook extends XC_MethodHook {
        private final String source;
        public MethodHook(String source){
            this.source = source;
        }

        protected void beforeHookedMethod(MethodHookParam param) throws Throwable {
            org.mozilla.javascript.Context rhino = org.mozilla.javascript.Context.enter();
            rhino.setOptimizationLevel(-1);
            try {
                Scriptable scope = rhino.initStandardObjects();
                ScriptableObject.putProperty(scope, "console", new Console());

                rhino.evaluateString(scope, source, null, 1, null);
                Function function = (Function) scope.get("before_func", scope);
                if (function != Scriptable.NOT_FOUND){
                    Log.i(TAG, "before call: " + function);
                    function.call(rhino, scope,scope ,new Object[]{param});
                }
            }catch (Exception e){
                Log.e(TAG,  e.toString());
                Log.e(TAG, Log.getStackTraceString(new Throwable()));
            }finally {
                org.mozilla.javascript.Context.exit();
            }
        }

        protected void afterHookedMethod(MethodHookParam param) throws Throwable {
            Log.getStackTraceString(new java.lang.Throwable());

            org.mozilla.javascript.Context rhino = org.mozilla.javascript.Context.enter();
            rhino.setOptimizationLevel(-1);
            try {
                Scriptable scope = rhino.initStandardObjects();
                rhino.evaluateString(scope, source, null, 1, null);
                ScriptableObject.putProperty(scope, "console", new Console());
                Function function = (Function) scope.get("after_func", scope);
                if (function != Scriptable.NOT_FOUND){
                    Log.i(TAG, "after call: " + function);
                    function.call(rhino, scope,scope ,new Object[]{param});
                }
            }catch (Exception e){
                Log.e(TAG,  e.toString());
                Log.e(TAG,  Log.getStackTraceString(new Throwable()));
            }finally {
                org.mozilla.javascript.Context.exit();
            }
        }
    }
}


