package com.contoso.droidvr

import android.app.Activity
import android.media.AudioRecord
import android.media.AudioTrack
import android.os.Bundle
import android.system.Os.setenv
import android.util.Log
import android.view.Surface
import android.view.SurfaceHolder
import android.view.SurfaceView
import android.view.WindowManager
import java.io.File

private const val TAG = "TAG1"

private var view: SurfaceView? = null
private var surfaceHolder: SurfaceHolder? = null
private var nativeHandle: Long = 0

private var audioTrack: AudioTrack? = null
private var audioRecord: AudioRecord? = null

// Activity lifecycle
external fun onCreate(
    obj: Activity,
    commandLineParams: String,
    refresh: Long,
    ss: Float,
    msaa: Long
): Long

external fun onStart(handle: Long, obj: Any)
external fun onResume(handle: Long)
external fun onPause(handle: Long)
external fun onStop(handle: Long)
external fun onDestroy(handle: Long)

// Surface lifecycle
external fun onSurfaceCreated(handle: Long, s: Surface?)
external fun onSurfaceChanged(handle: Long, s: Surface?)
external fun onSurfaceDestroyed(handle: Long)

open class BaseActivity : Activity(), SurfaceHolder.Callback {
    companion object {
        var instance: BaseActivity? = null

        init {
            System.loadLibrary("droidvr")
        }
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        Log.i(TAG, "----------------------------------------------------------------");
        Log.i(TAG, "CREATE2()");
        super.onCreate(savedInstanceState)

        view = SurfaceView(this)
        setContentView(view)
        view!!.holder.addCallback(this)

        // Force the screen to stay on, rather than letting it dim and shut off
        window.addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON)

        // Force screen brightness to stay at maximum
        val params = window.attributes
        params.screenBrightness = 1.0f
        window.attributes = params
    }

    fun create() {
        Log.v(TAG, "::create()")

        // make the directories
        File("/sdcard/Droid/Main").mkdirs()

        // copy assets
        AssetHelper.copyAsset(assets, "/sdcard/Droid/Main", "main.cfg")

        // read these from a file and pass through
        val commandLineParams = AssetHelper.readMultiLine("/sdcard/Droid/commandline.txt", "Droid")

        // set environment
        try {
            setenv("USER_FILES", "/sdcard/Droid", true);
            setenv("DROID_LIBDIR", applicationInfo.nativeLibraryDir, true)
        } catch (e: Exception) {
        }

        // parse the config file for these values
        val refresh: Long = 60 // Default to 60
        val ss = -1.0f
        val msaa: Long = 1 // default for both HMDs

        // create handle
        nativeHandle = onCreate(this, commandLineParams, refresh, ss, msaa)
    }

    override fun onStart() {
        Log.v(TAG, "!START")
        super.onStart()
        onStart(nativeHandle, this)
    }

    override fun onResume() {
        Log.v(TAG, "!RESUME")
        super.onResume()
        onResume(nativeHandle)
    }

    override fun onPause() {
        Log.v(TAG, "!PAUSE")
        onPause(nativeHandle)
        super.onPause()
    }

    override fun onStop() {
        Log.v(TAG, "!STOP")
        onStop(nativeHandle)
        super.onStop()
    }

    override fun onDestroy() {
        Log.v(TAG, "!DESTROY")
        if (surfaceHolder != null)
            onSurfaceDestroyed(nativeHandle)
        onDestroy(nativeHandle)
        super.onDestroy()
        nativeHandle = 0
    }

    override fun surfaceCreated(holder: SurfaceHolder) {
        Log.v(TAG, "SURFACE-CREATED")
        if (nativeHandle == 0L)
            return
        onSurfaceCreated(nativeHandle, holder.surface)
        surfaceHolder = holder
    }

    override fun surfaceChanged(holder: SurfaceHolder, format: Int, width: Int, height: Int) {
        Log.v(TAG, "SURFACE-CHANGED")
        if (nativeHandle == 0L)
            return
        onSurfaceChanged(nativeHandle, holder.surface)
        surfaceHolder = holder
    }

    override fun surfaceDestroyed(holder: SurfaceHolder?) {
        Log.v(TAG, "SURFACE-DESTROYED")
        if (nativeHandle == 0L)
            return
        onSurfaceDestroyed(nativeHandle)
        surfaceHolder = null
    }
}