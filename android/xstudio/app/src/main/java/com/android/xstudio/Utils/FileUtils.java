package com.android.xstudio.Utils;

import android.os.Environment;

import java.io.BufferedOutputStream;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.ObjectInput;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;

/**
 * Created by root on 2019/4/17.
 */

public class FileUtils {

    public static void save(String name, String value) {
        synchronized (FileUtils.class) {
            try {
                if (!Environment.MEDIA_MOUNTED.equals(Environment.getExternalStorageState())) {
                    return;
                }

                String path = Environment.getExternalStorageDirectory().getAbsolutePath();
                File file = new File(path + "/" + name);
                FileOutputStream fos = new  FileOutputStream(file);
                byte[] bytes = value.getBytes("UTF-8");
                fos.write(bytes, 0 , bytes.length);
                fos.close();

            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    }

    public static String read(String name) {
        synchronized (FileUtils.class) {
            try {
                if (!Environment.MEDIA_MOUNTED.equals(Environment.getExternalStorageState())) {
                    return null;
                }

                String path = Environment.getExternalStorageDirectory().getAbsolutePath();
                File file = new File(path + "/" + name);

                FileInputStream fis = (new FileInputStream(file));
                byte[] bytes = new byte[fis.available()];
                fis.read(bytes);
                fis.close();
                return new String(bytes, "UTF-8");
            } catch (Exception e) {
                e.printStackTrace();
            }
            return null;
        }
    }
}
