package com.android.xstudio;

import com.crossbowffs.remotepreferences.RemotePreferenceProvider;

/**
 * Created by root on 2019/4/27.
 */

public class XPreferenceProvider extends RemotePreferenceProvider {
    public XPreferenceProvider(){
        super("com.android.xstudio", new String[] {"main_prefs"});
    }
}
