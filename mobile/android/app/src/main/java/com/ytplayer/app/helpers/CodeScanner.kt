package com.youtube.media.controller.helpers

import android.app.Activity
import android.widget.Toast
import com.budiyev.android.codescanner.AutoFocusMode
import com.budiyev.android.codescanner.CodeScanner
import com.budiyev.android.codescanner.ScanMode
import java.net.MalformedURLException
import java.net.URL
import com.budiyev.android.codescanner.*


class CodeScanner(
    currentActivity: Activity,
    viewer: CodeScannerView,
    private val onQrScanned: (String) -> Unit
) {
    private var codeScanner: com.budiyev.android.codescanner.CodeScanner
    private var store: StoreService

    init {
        store = StoreService(currentActivity.baseContext)
        codeScanner = com.budiyev.android.codescanner.CodeScanner(currentActivity.baseContext, viewer)

        codeScanner.camera = com.budiyev.android.codescanner.CodeScanner.CAMERA_BACK
        codeScanner.formats = com.budiyev.android.codescanner.CodeScanner.ALL_FORMATS
        codeScanner.autoFocusMode = AutoFocusMode.SAFE
        codeScanner.scanMode = ScanMode.SINGLE
        codeScanner.isAutoFocusEnabled = true
        codeScanner.isFlashEnabled = false

        codeScanner.decodeCallback = DecodeCallback {
            currentActivity.runOnUiThread {
                onQrScanned(it.text)
            }
        }

        codeScanner.errorCallback = ErrorCallback {
            currentActivity.runOnUiThread {
                Toast.makeText(
                    currentActivity.baseContext,
                    "Camera initialization error: ${it.message}",
                    Toast.LENGTH_LONG
                ).show()
            }
        }
    }

    fun startPreview() {
        codeScanner.startPreview()
    }
}