package com.youtube.media.controller.work

import android.content.Context
import androidx.work.CoroutineWorker
import androidx.work.WorkerParameters
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.RequestBody.Companion.toRequestBody
import org.json.JSONObject

class RequestWorker(appContext: Context, params: WorkerParameters) : CoroutineWorker(appContext, params) {
    override suspend fun doWork(): Result {
        val base = inputData.getString("base") ?: return Result.failure()
        val url = inputData.getString("url") ?: return Result.failure()
        val path = inputData.getString("path") ?: return Result.failure()

        val body = JSONObject(mapOf("SourceUrl" to url)).toString()
            .toRequestBody("application/json; charset=utf-8".toMediaType())

        val req = Request.Builder().url("$base$path").post(body).build()
        val client = OkHttpClient()
        client.newCall(req).execute().use { res ->
            if (!res.isSuccessful) return Result.retry()
            res.body?.string()
        }
        return Result.success()
    }
}