package com.android.xstudio.service;

import android.content.SharedPreferences;
import android.text.TextUtils;
import android.util.Log;

import com.android.xstudio.Config;
import com.android.xstudio.Module;
import com.android.xstudio.Utils.FileUtils;
import com.android.xstudio.Utils.RsaUtils;

import java.util.List;

import de.robv.android.xposed.XSharedPreferences;
import redis.clients.jedis.Jedis;
import redis.clients.jedis.JedisPool;
import redis.clients.jedis.JedisPoolConfig;
import redis.clients.jedis.JedisPubSub;

/**
 * Created by Mi on 2019/4/14.
 */

public class XService {
    public  static final String TAG = "XSTUDIO";
    private static  XService instarnce;
    private static  JedisPool mpool = null;
    private static SharedPreferences mprefs;

    private String mhost;
    private int mport;
    private String mpswd;
    private String mpackage;

    private ICallback mCallback;


    private XService(){

        mprefs = Module.sPrefs;

        mhost = mprefs.getString(Config.SP_REDIS_HOST, "");
        mport = mprefs.getInt(Config.SP_REDIS_PORT, 0);
        mpswd = mprefs.getString(Config.SP_REDIS_PSWD, "");
        mpackage = mprefs.getString(Config.SP_PACKAGE, "");


        JedisPoolConfig config = new JedisPoolConfig();
        config.setMaxActive(64);
        config.setMaxIdle(32);

        if (TextUtils.isEmpty(mpswd)){
            mpool = new JedisPool(config, mhost, mport, 4000);
        }else {
            mpool = new JedisPool(config, mhost, mport, 4000, mpswd);
        }
    }

    public static XService getInstarnce(){
        if (instarnce == null){
            synchronized (XService.class){
                instarnce = new XService();
            }
        }
        return instarnce;
    }

    public void setCallback(ICallback callback){
        mCallback = callback;
    }

    public void sendData(String uuid, String data){
        Jedis jedis = null;
        try {
            String text = RsaUtils.encrypt(data);
            jedis = mpool.getResource();
            jedis.lpush(mpackage + ":response:" + uuid, text);
        }catch (Exception e){
            if (jedis != null) {
                mpool.returnBrokenResource(jedis);
            }
            Log.e(TAG,  Log.getStackTraceString(new Throwable()));
        }finally {
            if (jedis != null){
                mpool.returnResource(jedis);
            }
        }
    }

    public void publish(final String data){
        Thread thread = new Thread(new Runnable() {
            @Override
            public void run() {
                Jedis jedis = null;
                try {
                    jedis = mpool.getResource();
                    jedis.publish("console", data);
                }catch (Exception e){
                    if (jedis != null) {
                        mpool.returnBrokenResource(jedis);
                    }
                    Log.e(TAG,  e.toString());
                    Log.e(TAG,  Log.getStackTraceString(new Throwable()));
                }finally {
                    if (jedis != null){
                        mpool.returnResource(jedis);
                    }
                }
            }
        });
        thread.start();
    }

    public void start(){
        for (int i = 0; i < 5; i++) {
            Thread thread = new Thread(new Runnable() {
                @Override
                public void run() {
                    while (true){
                        Jedis jedis = null;
                        try {
                            jedis = mpool.getResource();
                            List<String> list = jedis.brpop(2, mpackage + ":request");
                            if (list != null){
                                String data = list.get(1);
                                if (mCallback != null && !TextUtils.isEmpty(data)){
                                    String text = RsaUtils.decrypt(data);
                                    mCallback.OnRequest(instarnce, text);
                                }
                            }
                        }catch (Exception e){
                            if (jedis != null) {
                                mpool.returnBrokenResource(jedis);
                            }
                            Log.e(TAG,  e.toString());
                            Log.e(TAG,  Log.getStackTraceString(new Throwable()));
                        }finally {
                            if (jedis != null){
                                mpool.returnResource(jedis);
                            }
                        }
                    }
                }
            });
            thread.start();
        }
    }

    public void subscribe(){
        Thread thread = new Thread(new Runnable() {
            @Override
            public void run() {
                Jedis jedis = null;
                while (jedis == null){

                    try {
                        jedis = mpool.getResource();
                        jedis.subscribe(new JedisPubSub() {
                            @Override
                            public void onMessage(String channel, String message) {
                                try{
                                    String text = RsaUtils.decrypt(message);
                                    if (! TextUtils.isEmpty(text)){
                                        Log.i(TAG, "onMessage: " + text);
                                        FileUtils.save("hooks.json", text);
                                    }
                                }catch (Exception e){
                                    Log.e(TAG,  e.toString());
                                    Log.e(TAG,  Log.getStackTraceString(new Throwable()));
                                }
                            }
                            @Override
                            public void onPMessage(String s, String s1, String s2) {

                            }

                            @Override
                            public void onSubscribe(String s, int i) {

                            }
                            @Override
                            public void onUnsubscribe(String s, int i) {

                            }

                            @Override
                            public void onPUnsubscribe(String s, int i) {

                            }

                            @Override
                            public void onPSubscribe(String s, int i) {


                            }
                        }, "hooks");
                    }catch (Exception e){
                        if (jedis != null){
                            mpool.returnBrokenResource(jedis);
                            jedis = null;
                        }
                        Log.e(TAG, e.toString());
                        Log.e(TAG,  Log.getStackTraceString(new Throwable()));
                    }finally {
                        if (jedis != null){
                            mpool.returnResource(jedis);
                            jedis = null;
                        }
                        try {
                            Thread.sleep(5000);
                        } catch (InterruptedException e) {
                            Log.e(TAG,  e.toString());
                            Log.e(TAG,  Log.getStackTraceString(new Throwable()));
                        }
                    }
                }
            }
        });
        thread.setPriority(Thread.MAX_PRIORITY);
        thread.start();
    }
}


