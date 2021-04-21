package com.contoso.droidnet

import android.content.res.AssetManager
import android.util.Log
import java.io.File
import java.io.FileOutputStream
import java.io.InputStream
import java.io.OutputStream

class AssetHelper {
    companion object {
        fun printFolder(path: String, recurse: Boolean = true) {
            val file = File(path)
            val files: Array<File> = file.listFiles() ?: return
            for (i in files.indices) {
                val subPath: String = files[i].absolutePath
                Log.i("TAG1", subPath)
                if (recurse)
                    printFolder(subPath, recurse)
            }
        }

        fun copyAsset(
            assets: AssetManager,
            path: String,
            name: String,
            fromName: String?,
            force: Boolean = false
        ) {
            val f = File("$path/$name")
            if (!f.exists() || force) {
                val fullName = "$path/$name"
                val directory = fullName.substring(0, fullName.lastIndexOf("/"))
                File(directory).mkdirs()
                copyAsset_(assets, fromName ?: fromName, "$path/$name")
            }
        }

        private fun copyAsset_(assets: AssetManager, name_in: String?, name_out: String?) {
            try {
                val in_: InputStream = assets.open(name_in!!)
                val out: OutputStream = FileOutputStream(name_out)
                copyStream(in_, out)
                out.close()
                in_.close()
            } catch (e: Exception) {
                e.printStackTrace()
            }
        }

        private fun copyStream(in_: InputStream, out: OutputStream) {
            val buf = ByteArray(1024)
            while (true) {
                val count = in_.read(buf)
                if (count <= 0) break
                out.write(buf, 0, count)
            }
        }
    }
}