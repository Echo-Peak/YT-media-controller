package com.youtube.media.controller

import android.Manifest
import android.content.pm.PackageManager
import android.os.Bundle
import android.util.Log
import android.widget.TextView
import android.widget.Toast
import androidx.appcompat.app.AppCompatActivity
import androidx.core.app.ActivityCompat
import androidx.core.content.ContextCompat
import com.budiyev.android.codescanner.CodeScannerView
import com.youtube.media.controller.helpers.CodeScanner
import com.youtube.media.controller.helpers.StorePref
import com.youtube.media.controller.helpers.StoreService
import com.youtube.media.controller.helpers.Validate


class MainActivity : AppCompatActivity() {
    private lateinit var scanner: CodeScanner
    private lateinit var store: StoreService
    private val logTag = "MainActivity"
    private var httpService: HttpHelper? = null
    private val CAMERA_PERMISSION_REQUEST = 100

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        store = StoreService(this)
        setContentView(R.layout.activity_mainv2)
        loadCachedBackend()
        checkCameraPermission()
    }

    override fun onResume(){
        super.onResume()
    }

    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<out String>,
        grantResults: IntArray
    ) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        if (requestCode == CAMERA_PERMISSION_REQUEST &&
            grantResults.isNotEmpty() &&
            grantResults[0] == PackageManager.PERMISSION_GRANTED) {
            setupScanner()
        } else {
            Toast.makeText(this, "Camera permission is required", Toast.LENGTH_LONG).show()
        }
    }

    private fun checkCameraPermission() {
        if (ContextCompat.checkSelfPermission(this, Manifest.permission.CAMERA)
            != PackageManager.PERMISSION_GRANTED) {

            ActivityCompat.requestPermissions(this, arrayOf(Manifest.permission.CAMERA), CAMERA_PERMISSION_REQUEST)
        } else {
            setupScanner()
        }
    }

    private fun setupScanner() {
        val scannerView = findViewById<CodeScannerView>(R.id.scanner_view)
        scanner = CodeScanner(this, scannerView) { qrText ->
            Log.d(logTag, "QR code scanned: $qrText")
            if (Validate().isLocalDeviceUrl(qrText)){
                store.setKey(StorePref.BACKEND_SERVER_URL, qrText)
                updateLogUI(qrText)
            } else{
                updateLogUI("Unknown")
            }

        }
        scannerView.setOnClickListener { scanner.startPreview() }
    }

    private fun updateLogUI(message: String) {
        findViewById<TextView>(R.id.PluginLogTextView).text = message;
    }

    private fun loadCachedBackend() {
        // TODO: Add healthcheck endpoient and reflect state to UI
        store.getKey(StorePref.BACKEND_SERVER_URL)?.let { host ->
            updateLogUI("Current host: $host")
            httpService = HttpHelper(host)
        } ?: updateLogUI("Host URL is not set")
    }
}