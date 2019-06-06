package com.android.xstudio;

import android.app.Application;
import android.content.Context;
import android.content.SharedPreferences;
import android.content.pm.ApplicationInfo;
import android.database.sqlite.SQLiteDatabase;
import android.icu.text.DateFormat;
import android.util.Base64;
import android.util.Log;

import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONArray;
import com.android.xstudio.hooks.HideHooks;
import com.android.xstudio.hooks.UserHooks;
import com.android.xstudio.model.Request;
import com.android.xstudio.model.Response;
import com.android.xstudio.service.ICallback;
import com.android.xstudio.service.XEngine;
import com.android.xstudio.service.XService;
import com.crossbowffs.remotepreferences.RemotePreferences;

import org.joor.Reflect;

import java.io.File;
import java.lang.reflect.Field;
import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import de.robv.android.xposed.IXposedHookLoadPackage;
import de.robv.android.xposed.IXposedHookZygoteInit;
import de.robv.android.xposed.XC_MethodHook;
import de.robv.android.xposed.XSharedPreferences;
import de.robv.android.xposed.XposedBridge;
import de.robv.android.xposed.XposedHelpers;
import de.robv.android.xposed.callbacks.XC_LoadPackage;

/**
 * Created by Mi on 2019/4/13.
 */
public class Module extends XC_MethodHook implements IXposedHookLoadPackage, IXposedHookZygoteInit {
    public static final String TAG = "XSTUDIO";
    public static SharedPreferences sPrefs;

    @Override
    public void handleLoadPackage(final XC_LoadPackage.LoadPackageParam lpparam) throws Throwable {
        if (lpparam.appInfo == null || (lpparam.appInfo.flags & (ApplicationInfo.FLAG_SYSTEM | ApplicationInfo.FLAG_UPDATED_SYSTEM_APP)) != 0) {
            return;
        }

        if (sPrefs == null){
            Context systemContext = (Context) XposedHelpers.callMethod(XposedHelpers.callStaticMethod( XposedHelpers.findClass("android.app.ActivityThread", lpparam.classLoader), "currentActivityThread"), "getSystemContext" );
            sPrefs = new RemotePreferences(systemContext, "com.android.xstudio", "main_prefs");
        }


        Log.d(TAG, "handleLoadPackage: " + sPrefs.getString(Config.SP_PACKAGE, ""));

        if (! sPrefs.getBoolean(Config.SP_IS_START, false)) {
           return;
        }


        if ("de.robv.android.xposed.installer".equals(lpparam.packageName)) {
            return;
        }if ("com.android.xstudio".equals(lpparam.packageName)){
            return;
        }



        if (!sPrefs.getString(Config.SP_PACKAGE, "").equals(lpparam.packageName)) {
            return;
        }
        if (!sPrefs.getString(Config.SP_PACKAGE, "").equals(lpparam.processName)) {
            return;
        }

        //HideHooks.initAllHooks(lpparam);

        XposedHelpers.findAndHookMethod(Application.class, "attach", Context.class, new XC_MethodHook() {
            @Override
            protected void afterHookedMethod(MethodHookParam param) throws Throwable {
                final Context ctx = (Context) param.args[0];
                if (ctx != null) {
                    try{
                        UserHooks.initAllHooks(ctx, lpparam);
                        XService x = XService.getInstarnce();
                        x.setCallback(new ICallback() {
                            @Override
                            public void OnRequest(XService service, String message) {
                                //Log.i(TAG, "OnRequest: " + message);
                                try {
                                    Request req = Request.fromString(message);
                                    Response res = XEngine.request(ctx, lpparam, req.source, req.args);
                                    //Log.i(TAG, "OnRequest: " + res.toString());
                                    service.sendData(req.uuid, res.toString());
                                } catch (Exception e) {
                                    Log.e(TAG,  e.toString());
                                    Log.e(TAG,  Log.getStackTraceString(new Throwable()));
                                }
                                Base64.decode("", 0);
                            }
                        });
                        x.subscribe();
                        x.start();
                    }catch (Exception ex){
                        Log.e(TAG, "attach: " + ex.getMessage());
                    }

                }
            }
        });
    }

    @Override
    public void initZygote(StartupParam startupParam) throws Throwable {
        Log.i(TAG, "initZygote");
    }
}
