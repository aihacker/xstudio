package com.android.xstudio;

import android.app.Activity;
import android.content.Context;
import android.content.DialogInterface;
import android.content.SharedPreferences;
import android.content.pm.PackageInfo;
import android.os.Handler;
import android.os.Message;
import android.support.v4.content.ContextCompat;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.text.InputType;
import android.text.TextUtils;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;
import android.widget.CompoundButton;
import android.widget.TextView;
import android.widget.Toast;

import com.android.xstudio.Utils.RsaUtils;
import com.crossbowffs.remotepreferences.RemotePreferences;
import com.qmuiteam.qmui.qqface.IQMUIQQFaceManager;
import com.qmuiteam.qmui.util.QMUIDisplayHelper;
import com.qmuiteam.qmui.util.QMUIStatusBarHelper;
import com.qmuiteam.qmui.widget.QMUITopBar;
import com.qmuiteam.qmui.widget.QMUITopBarLayout;
import com.qmuiteam.qmui.widget.dialog.QMUIDialog;
import com.qmuiteam.qmui.widget.dialog.QMUIDialogAction;
import com.qmuiteam.qmui.widget.dialog.QMUITipDialog;
import com.qmuiteam.qmui.widget.grouplist.QMUICommonListItemView;
import com.qmuiteam.qmui.widget.grouplist.QMUIGroupListView;
import com.squareup.okhttp.internal.http.OkHeaders;

import java.util.ArrayList;
import java.util.List;

import okio.Okio;
import redis.clients.jedis.Jedis;

public class MainActivity extends AppCompatActivity {

    private static String TAG = "reap";
    private QMUITopBar mTopBar;
    private QMUIGroupListView mGroupListView;
    private SharedPreferences mPrefs;
    private Context mContext;

    // Used to load the 'native-lib' library on application startup.
    static {
        System.loadLibrary("native-lib");
    }



    @Override
    protected void onCreate(Bundle savedInstanceState) {


        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        mContext = this;
        mPrefs =  new RemotePreferences(this, "com.android.xstudio", "main_prefs");;//this.getSharedPreferences("config", Context.MODE_PRIVATE);

        if ( mPrefs.getString(Config.SP_REDIS_HOST,null) == null){
            mPrefs.edit().putString(Config.SP_REDIS_HOST, "10.0.0.100").commit();
        }

        if ( mPrefs.getInt(Config.SP_REDIS_PORT, 0) == 0){
            mPrefs.edit().putInt(Config.SP_REDIS_PORT, 6379).commit();
        }

        if ( mPrefs.getString(Config.SP_REDIS_PSWD,null) == null){
            mPrefs.edit().putString(Config.SP_REDIS_PSWD, "android").commit();
        }

        QMUIStatusBarHelper.translucent(this);
        mTopBar = (QMUITopBar) findViewById(R.id.topbar);
        initTopBar();
        mGroupListView = (QMUIGroupListView) findViewById(R.id.groupListView);
        initGroupListView();
    }

    private void initTopBar() {
        mTopBar.addLeftBackImageButton().setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                finish();
            }
        });
        mTopBar.setTitle("XSTUDIO");
    }

    private String[] getInstalledApps() {
        ArrayList<String> appsList = new ArrayList<>();
        List<PackageInfo> packs = this.getPackageManager().getInstalledPackages(0);

        for (int i = 0; i < packs.size(); i++) {
            android.content.pm.PackageInfo p = packs.get(i);
            if ((p.applicationInfo.flags & 129) == 0) {
                appsList.add(p.packageName);
            }
        }

        return appsList.toArray(new String[0]);
    }

    private void showEditTextDialog(String title, CharSequence text, final QMUICommonListItemView item) {
        final QMUIDialog.EditTextDialogBuilder builder = new QMUIDialog.EditTextDialogBuilder(this);
        builder .setTitle(title)
                .setDefaultText(text)
                .setInputType(InputType.TYPE_TEXT_FLAG_CAP_CHARACTERS)
                .addAction("取消", new QMUIDialogAction.ActionListener() {
                    @Override
                    public void onClick(QMUIDialog dialog, int index) {
                        dialog.dismiss();
                    }
                })
                .addAction("确定", new QMUIDialogAction.ActionListener() {
                    @Override
                    public void onClick(QMUIDialog dialog, int index) {
                        CharSequence text = builder.getEditText().getText();
                        String tag = (String) item.getTag();
                        if (Config.SP_REDIS_PORT.equals(tag)){
                            int port = 6379;
                            try {
                                port = Integer.parseInt(text.toString());
                            }catch (Exception e){
                                port = 6379;
                            }
                            mPrefs.edit().putInt(tag, port).commit();
                            item.setDetailText(port + "");
                        }else {
                            mPrefs.edit().putString(tag, text.toString()).commit();
                            item.setDetailText(text);
                        }

                        dialog.dismiss();
                    }
                })
                .create(com.qmuiteam.qmui.R.style.QMUI_Dialog).show();
    }

    private void showSingleChoiceDialog(CharSequence text, final QMUICommonListItemView itemView) {
        final String[] items = getInstalledApps();

        int checkedIndex = -1;
        for (int i = 0; i < items.length; i++) {
            if (items[i].equals(text.toString()))
                  checkedIndex = i;
        }

        new QMUIDialog.CheckableDialogBuilder(this)
                .setCheckedIndex(checkedIndex)
                .addItems(items, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        String tag = (String) itemView.getTag();
                        mPrefs.edit().putString(tag, items[which]).commit();
                        itemView.setDetailText(items[which]);
                        dialog.dismiss();
                    }
                })
                .create(com.qmuiteam.qmui.R.style.QMUI_Dialog).show();
    }

    private void TestRedisClient(){

        final String host = mPrefs.getString(Config.SP_REDIS_HOST, "");
        final int port = (mPrefs.getInt(Config.SP_REDIS_PORT, 0));
        final String pswd = mPrefs.getString(Config.SP_REDIS_PSWD, "");

        final Handler handler = new Handler(){
            @Override
            public void handleMessage(Message msg){
                super.handleMessage(msg);
                if(msg.what == 10086){
                    final QMUITipDialog tip =   new QMUITipDialog.Builder(mContext)
                            .setTipWord((String)msg.obj)
                            .create();
                    tip.show();
                    postDelayed(new Runnable() {
                        @Override
                        public void run() {
                            tip.dismiss();
                        }
                    }, 1500);
                }
            }
        };

        final Thread thread = new Thread(new Runnable(){
            @Override
            public void run() {
                Jedis jedis = null;
                Message msg = new Message();
                msg.what = 10086;

                try {
                    jedis = new Jedis(host, port, 2000);
                    if (! TextUtils.isEmpty(pswd))
                        jedis.auth(pswd);
                    msg.obj = jedis.ping();
                }catch (Exception e){
                    msg.obj = e.getMessage();
                    if (jedis != null && jedis.isConnected()){
                        jedis.disconnect();
                    }
                }

                handler.sendMessage(msg);
            }
        });

        thread.start();
    }

    private void initGroupListView() {
        View.OnClickListener onClickListener = new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (v instanceof QMUICommonListItemView) {
                    QMUICommonListItemView item = (QMUICommonListItemView) v;
                    String tag = (String) item.getTag();
                    String name = (String) item.getText();

                    if (Config.SP_REDIS_HOST.equals(tag)){
                        showEditTextDialog(name, item.getDetailText(), item);
                    }

                    if (Config.SP_REDIS_PORT.equals(tag)){
                        showEditTextDialog(name, item.getDetailText(), item);
                    }

                    if (Config.SP_REDIS_PSWD.equals(tag)){
                        showEditTextDialog(name, item.getDetailText(), item);
                    }

                    if (Config.SP_PACKAGE.equals(tag)){
                        showSingleChoiceDialog(item.getDetailText(), item);
                    }

                    if (Config.SP_HOOKS.equals(tag)){
                        TestRedisClient();
                    }

                    CharSequence text = ((QMUICommonListItemView) v).getText();
                    if (((QMUICommonListItemView) v).getAccessoryType() == QMUICommonListItemView.ACCESSORY_TYPE_SWITCH) {
                        ((QMUICommonListItemView) v).getSwitch().toggle();
                    }
                }
            }
        };

        QMUICommonListItemView hostView = mGroupListView.createItemView("主机");
        //hostView.setOrientation(QMUICommonListItemView.HORIZONTAL);
        hostView.setTag(Config.SP_REDIS_HOST);
        hostView.setAccessoryType(QMUICommonListItemView.ACCESSORY_TYPE_CHEVRON);
        hostView.setDetailText(mPrefs.getString(Config.SP_REDIS_HOST,""));

        QMUICommonListItemView portView = mGroupListView.createItemView("端口");
        //portView.setOrientation(QMUICommonListItemView.HORIZONTAL);
        portView.setTag(Config.SP_REDIS_PORT);
        portView.setAccessoryType(QMUICommonListItemView.ACCESSORY_TYPE_CHEVRON);
        portView.setDetailText(mPrefs.getInt(Config.SP_REDIS_PORT,0) + "");

        QMUICommonListItemView pswdView = mGroupListView.createItemView("密码");
        //pswdView.setOrientation(QMUICommonListItemView.HORIZONTAL);
        pswdView.setTag(Config.SP_REDIS_PSWD);
        pswdView.setAccessoryType(QMUICommonListItemView.ACCESSORY_TYPE_CHEVRON);
        pswdView.setDetailText(mPrefs.getString(Config.SP_REDIS_PSWD,""));

        QMUICommonListItemView packageView = mGroupListView.createItemView("应用");
        //pswdView.setOrientation(QMUICommonListItemView.HORIZONTAL);
        packageView.setTag(Config.SP_PACKAGE);
        packageView.setAccessoryType(QMUICommonListItemView.ACCESSORY_TYPE_CHEVRON);
        packageView.setDetailText(mPrefs.getString(Config.SP_PACKAGE, ""));

        QMUICommonListItemView debugView = mGroupListView.createItemView("调用");
        debugView.setAccessoryType(QMUICommonListItemView.ACCESSORY_TYPE_SWITCH);
        debugView.getSwitch().setChecked(mPrefs.getBoolean(Config.SP_IS_DEBUG,false));
        debugView.getSwitch().setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
            @Override
            public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                mPrefs.edit().putBoolean(Config.SP_IS_DEBUG, isChecked).commit();
            }
        });


        QMUICommonListItemView startView = mGroupListView.createItemView("运行");
        startView.setAccessoryType(QMUICommonListItemView.ACCESSORY_TYPE_SWITCH);
        startView.getSwitch().setChecked(mPrefs.getBoolean(Config.SP_IS_START,false));
        startView.getSwitch().setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {
            @Override
            public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                mPrefs.edit().putBoolean(Config.SP_IS_START, isChecked).commit();
            }
        });

        QMUICommonListItemView syncView = mGroupListView.createItemView("测试");
        //pswdView.setOrientation(QMUICommonListItemView.HORIZONTAL);

        syncView.setTag(Config.SP_HOOKS);
        syncView.setAccessoryType(QMUICommonListItemView.ACCESSORY_TYPE_CHEVRON);

        QMUIGroupListView.newSection(mContext)
                .addItemView(hostView, onClickListener)
                .addItemView(portView, onClickListener)
                .addItemView(pswdView, onClickListener)
                .addTo(mGroupListView);

        QMUIGroupListView.newSection(mContext)
                .setTitle("")
                .addItemView(packageView, onClickListener)
                .addTo(mGroupListView);

        QMUIGroupListView.newSection(mContext)
                .setTitle("")
                .addItemView(syncView, onClickListener)
                .addTo(mGroupListView);

        QMUIGroupListView.newSection(mContext)
                .setTitle("")
                .addItemView(startView, onClickListener)
                //.addItemView(debugView, onClickListener)
                .addTo(mGroupListView);

        QMUIGroupListView.newSection(mContext)
                .setTitle("By reap(qq:12300735)")
                .addTo(mGroupListView);
    }
    /**
     * A native method that is implemented by the 'native-lib' native library,
     * which is packaged with this application.
     */
    public native String stringFromJNI();
}
