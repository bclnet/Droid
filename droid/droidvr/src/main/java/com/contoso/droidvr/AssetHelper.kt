package com.contoso.droidvr

import android.content.res.AssetManager
import android.util.Log
import java.io.*

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

        fun readMultiLine(file: String, default: String): String {
            if (!File(file).exists())
                return default
            try {
                val br = BufferedReader(FileReader(file))
                var s: String
                val sb = StringBuilder(0)
                while (br.readLine().also { s = it } != null) sb.append("$s ")
                br.close()
                return sb.toString()
            } catch (e: FileNotFoundException) {
                // TODO Auto-generated catch block
                e.printStackTrace()
            } catch (e: IOException) {
                // TODO Auto-generated catch block
                e.printStackTrace()
            }
            return default
        }

        fun copyAsset(
            assets: AssetManager,
            path: String,
            name: String,
            fromName: String? = null,
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
            val buf = ByteArray(4096)
            while (true) {
                val count = in_.read(buf)
                if (count <= 0) break
                out.write(buf, 0, count)
            }
        }
    }
}