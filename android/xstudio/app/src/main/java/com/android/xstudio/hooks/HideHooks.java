package com.android.xstudio.hooks;

import android.content.pm.ApplicationInfo;
import android.content.pm.PackageInfo;
import android.text.TextUtils;
import android.util.Log;

import java.io.BufferedInputStream;
import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.lang.reflect.Modifier;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Map;

import de.robv.android.xposed.XC_MethodHook;
import de.robv.android.xposed.XC_MethodReplacement;
import de.robv.android.xposed.XposedBridge;
import de.robv.android.xposed.XposedHelpers;
import de.robv.android.xposed.callbacks.XC_LoadPackage;

import static de.robv.android.xposed.XposedHelpers.findAndHookMethod;

/**
 * Created by root on 2019/4/17.
 */

public class HideHooks {
    public static void initAllHooks(final XC_LoadPackage.LoadPackageParam lpparam) {
        XC_MethodHook hookClass = new XC_MethodHook() {
            @Override
            protected void beforeHookedMethod(MethodHookParam param) throws Throwable {
                String name = (String) param.args[0];
                if (name != null && name.startsWith("de.robv.android.xposed.Xposed")) {
                    param.setThrowable(new ClassNotFoundException(name));
                }
            }
        };

        findAndHookMethod(ClassLoader.class, "loadClass", String.class, boolean.class, hookClass);
        findAndHookMethod(Class.class, "forName", String.class, boolean.class, ClassLoader.class, hookClass);

        XC_MethodHook hookStack = new XC_MethodHook() {
            @Override
            protected void afterHookedMethod(MethodHookParam param) {
                StackTraceElement[] elements = (StackTraceElement[]) param.getResult();
                List<StackTraceElement> clone = new ArrayList<>();
                for (StackTraceElement element : elements) {
                    if (!element.getClassName().toLowerCase().contains("xposed")) {
                        clone.add(element);
                    }
                }
                param.setResult(clone.toArray(new StackTraceElement[0]));
            }
        };

        findAndHookMethod(
                Throwable.class,
                "getStackTrace",
                hookStack
        );
        findAndHookMethod(
                Thread.class,
                "getStackTrace",
                hookStack
        );

        findAndHookMethod(
                "android.app.ApplicationPackageManager",
                lpparam.classLoader,
                "getInstalledPackages",
                int.class,
                new XC_MethodHook() {
                    @Override
                    protected void afterHookedMethod(MethodHookParam param) {
                        List<PackageInfo> apps = (List<PackageInfo>) param.getResult();
                        List<PackageInfo> clone = new ArrayList<>();
                        final int len = apps.size();
                        for (int i = 0; i < len; i++) {
                            PackageInfo app = apps.get(i);
                            if (!app.packageName.toLowerCase().contains("xposed")) {
                                clone.add(app);
                            }
                        }
                        param.setResult(clone);
                    }
                }
        );

        findAndHookMethod(
                "android.app.ApplicationPackageManager",
                lpparam.classLoader,
                "getInstalledApplications",
                int.class,
                new XC_MethodHook() {
                    @Override
                    protected void afterHookedMethod(MethodHookParam param) {
                        List<ApplicationInfo> apps = (List<ApplicationInfo>) param.getResult();
                        List<ApplicationInfo> clone = new ArrayList<>();
                        final int len = apps.size();
                        for (int i = 0; i < len; i++) {
                            ApplicationInfo app = apps.get(i);
                            boolean shouldRemove = app.metaData != null && app.metaData.getBoolean("xposedmodule") ||
                                    app.packageName != null && app.packageName.toLowerCase().contains("xposed") ||
                                    app.className != null && app.className.toLowerCase().contains("xposed") ||
                                    app.processName != null && app.processName.toLowerCase().contains("xposed");
                            if (!shouldRemove) {
                                clone.add(app);
                            }
                        }
                        param.setResult(clone);
                    }
                }

        );


        XposedBridge.hookAllMethods(System.class, "getenv",
                new XC_MethodHook() {
                    @Override
                    protected void afterHookedMethod(MethodHookParam param) {
                        if (param.args == null || param.args.length == 0) {
                            Map<String, String> res = (Map<String, String>) param.getResult();
                            String classpath = res.get("CLASSPATH");
                            param.setResult(filter(classpath));
                        } else if ("CLASSPATH".equals(param.args[0])) {
                            String classpath = (String) param.getResult();
                            param.setResult(filter(classpath));
                        }
                    }

                    private String filter(String s) {
                        List<String> list = Arrays.asList(s.split(":"));
                        List<String> clone = new ArrayList<>();
                        for (int i = 0; i < list.size(); i++) {
                            if (!list.get(i).toLowerCase().contains("xposed")) {
                                clone.add(list.get(i));
                            }
                        }
                        StringBuilder res = new StringBuilder();
                        for (int i = 0; i < clone.size(); i++) {
                            res.append(clone);
                            if (i != clone.size() - 1) {
                                res.append(":");
                            }
                        }
                        return res.toString();
                    }
                }
        );

        XposedBridge.hookAllConstructors(FileInputStream.class, new XC_MethodHook() {
            @Override
            protected void afterHookedMethod(MethodHookParam param) throws Throwable {
                String path = null;
                if (param.args[0] instanceof  String){
                    path = (String) param.args[0];
                }
                if (param.args[0] instanceof  File){
                    path = ((File) param.args[0]).getName();
                }

                if (path != null && path.matches("/proc/[0-9A-Za-z_]+/maps")){
                    FileInputStream fis = (FileInputStream) param.thisObject;
                    try {
                        ByteArrayOutputStream out = new ByteArrayOutputStream();
                        byte[] buffer = new byte[1024];
                        int length;
                        while ((length = fis.read(buffer)) != -1) {
                            out.write(buffer, 0, length);
                        }
                        fis.close();

                        String text = out.toString("UTF-8");
                        if (text.contains("xposed")){
                            text = text.replaceAll("xposed","desopx");
                            text = text.replaceAll("Xposed","desopX");
                            param.setResult(new ByteArrayInputStream(text.getBytes("UTF-8")));
                        }
                    }catch (Exception e){

                    }
                }
            }
        });



        Class<?> clazz = null;
        try {
            clazz = Runtime.getRuntime().exec("echo").getClass();
        } catch (IOException ignore) {

        }
        if (clazz != null) {
            XposedHelpers.findAndHookMethod(
                    clazz,
                    "getInputStream",
                    new XC_MethodHook() {
                        @Override
                        protected void afterHookedMethod(MethodHookParam param) {
                            InputStream is = (InputStream) param.getResult();
                            try {
                                ByteArrayOutputStream out = new ByteArrayOutputStream();
                                byte[] buffer = new byte[1024];
                                int length;
                                while ((length = is.read(buffer)) != -1) {
                                    out.write(buffer, 0, length);
                                }
                                is.close();
                                String text = out.toString("UTF-8");
                                if (text.contains("xposed")){
                                    text = text.replaceAll("xposed","desopx");
                                    text = text.replaceAll("Xposed","desopX");
                                    param.setResult(new ByteArrayInputStream(text.getBytes("UTF-8")));
                                }
                            }catch (Exception e){

                            }
                        }
                    }
            );
        }
    }

}
