package com.android.xstudio.Utils;

import android.util.Base64;
import android.util.Log;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.security.KeyFactory;
import java.security.NoSuchAlgorithmException;
import java.security.interfaces.RSAPublicKey;
import java.security.spec.InvalidKeySpecException;
import java.security.spec.X509EncodedKeySpec;

import javax.crypto.Cipher;
import javax.crypto.NoSuchPaddingException;

/**
 * Created by root on 2019/4/21.
 */

public class RsaUtils {
    private static RSAPublicKey Key = null;

    static {
        byte[] bytes = Base64.decode("MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCBXjdBBo7uZ2gjOTYy3RXd6FMnw3i4C4AY0/ui7YX2ooLy8+NfWW0xXGqDCiu2L7RQKgNdkMcytS7lYjaVXzsbBhB2e7Tvb+IlkG0hO5+7wS80vQ8D8nhiogOSP9rEQAVhgcD2a/1HLG+pHKZveLOmalyhtKvsH16NVexfCfinUwIDAQAB", 2);
        try {
            Key = (RSAPublicKey) KeyFactory.getInstance("RSA").generatePublic(new X509EncodedKeySpec(bytes));
        } catch (Exception e) {
            e.printStackTrace();
        }


    }

    public static byte[] decrypt(byte[] content) {
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        try {
            Cipher cipher = Cipher.getInstance("RSA/ECB/PKCS1Padding");
            cipher.init(Cipher.DECRYPT_MODE, Key);
            int length = content.length;
            int offset = 0;

            while (length - offset > 0) {
                byte[] cache = cipher.doFinal(content, offset, length - offset > 128 ? 128 : length - offset);
                offset += 128;
                baos.write(cache);
            }
            byte[] bytes = baos.toByteArray();
            baos.close();
            return bytes;
        } catch (Exception e) {
            Log.d("XSTUDIO", e.getMessage());
        } finally {
            try {
                baos.close();
            } catch (Exception e) {
                Log.d("XSTUDIO", e.getMessage());
            }
        }
        return null;
    }

    public static String decrypt(String content) {
        try {
            byte[] bytes = Base64.decode(content, 2);
            byte[] result=decrypt(bytes);
            return new String(result, "UTF-8");
        }catch (Exception e){
            Log.d("XSTUDIO", e.getMessage());
        }
        return null;
    }


    public static byte[] encrypt(byte[] content) {
        ByteArrayOutputStream baos = new ByteArrayOutputStream();
        try {
            Cipher cipher = Cipher.getInstance("RSA/ECB/PKCS1Padding");
            cipher.init(Cipher.ENCRYPT_MODE, Key);
            int length = content.length;
            int offset = 0;

            while (length - offset > 0) {
                byte[] cache = cipher.doFinal(content, offset, length - offset > 117 ? 117 : length - offset);
                offset += 117;
                baos.write(cache);
            }
            byte[] bytes = baos.toByteArray();
            baos.close();
            return bytes;

        } catch (Exception e) {
            Log.d("XSTUDIO", e.getMessage());
        } finally {
            try {
                baos.close();
            } catch (Exception e) {
                Log.d("XSTUDIO", e.getMessage());
            }
        }
        return null;
    }

    public static String encrypt(String content) {
        try {
            byte[] bytes = content.getBytes("UTF-8");
            byte[] result= encrypt(bytes);
            return Base64.encodeToString(result, 2);
        }catch (Exception e){
            Log.d("XSTUDIO", e.getMessage());
        }
        return null;
    }
}
