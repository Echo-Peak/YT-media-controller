package com.youtube.media.controller.helpers

class Validate {

    fun isYouTubeUrl(url: String): Boolean {
        return url.contains("youtube.com/watch")  || url.contains("youtube.com/shorts")
    }

    fun isLocalDeviceUrl(url: String): Boolean {
        return url.startsWith("http://192.");
    }

}