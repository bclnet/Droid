package com.contoso.droid

import android.Manifest
import android.content.pm.PackageManager
import android.os.Bundle
import android.util.Log
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import com.contoso.droidnet.DroidNet
import com.contoso.droidvr.OVRActivity
import kotlin.system.exitProcess

private const val TAG = "TAG1"
private val droidNet = DroidNet()
private var permissionCount = 0

private const val READ_EXTERNAL_STORAGE_PERMISSION_ID = 1
private const val WRITE_EXTERNAL_STORAGE_PERMISSION_ID = 2

class MainActivity : OVRActivity() {
    external fun stringFromJNI(): String

    companion object {
        init {
            System.loadLibrary("native-lib")
        }
    }

    fun shutdown() {
        Log.i(TAG, "SHUTDOWN()")
        exitProcess(0)
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        Log.i(TAG, "----------------------------------------------------------------");
        Log.i(TAG, "CREATE()");
        droidNet.mono(this, "AndroidConsole", "AndroidConsole.dll")
        super.onCreate(savedInstanceState)

        checkPermissionsAndInitialize();
    }

    // Initializes the Activity only if the permission has been granted.
    private fun checkPermissionsAndInitialize() {
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.WRITE_EXTERNAL_STORAGE)
            != PackageManager.PERMISSION_GRANTED
        )
            ActivityCompat.requestPermissions(
                this,
                arrayOf(Manifest.permission.WRITE_EXTERNAL_STORAGE),
                WRITE_EXTERNAL_STORAGE_PERMISSION_ID
            )
        else permissionCount++
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.READ_EXTERNAL_STORAGE)
            != PackageManager.PERMISSION_GRANTED
        )
            ActivityCompat.requestPermissions(
                this, arrayOf(Manifest.permission.READ_EXTERNAL_STORAGE),
                READ_EXTERNAL_STORAGE_PERMISSION_ID
            )
        else permissionCount++

        // Permissions have already been granted.
        if (permissionCount == 2)
            create()
    }

    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<out String>,
        grantResults: IntArray
    ) {
        if (requestCode == READ_EXTERNAL_STORAGE_PERMISSION_ID) {
            if (grantResults.isNotEmpty() && grantResults[0] == PackageManager.PERMISSION_GRANTED)
                permissionCount++
            else exitProcess(0)
        }
        if (requestCode == WRITE_EXTERNAL_STORAGE_PERMISSION_ID) {
            if (grantResults.isNotEmpty() && grantResults[0] == PackageManager.PERMISSION_GRANTED)
                permissionCount++
            else exitProcess(0)
        }
        checkPermissionsAndInitialize()
    }
}