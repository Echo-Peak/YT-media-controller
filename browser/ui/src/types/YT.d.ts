declare namespace YT {
  /** Ready-to-embed IFrame player instance */
  class Player {
    /** Create a new player in the given element (or element ID) */
    constructor(element: HTMLElement | string, options?: PlayerOptions);

    /* ───────────────────── Queueing ───────────────────── */
    cueVideoById(videoId: string, startSeconds?: number): void;
    cueVideoById(vars: CueVideoByIdVars): void;

    loadVideoById(videoId: string, startSeconds?: number): void;
    loadVideoById(vars: CueVideoByIdVars): void;

    cueVideoByUrl(mediaContentUrl: string, startSeconds?: number): void;
    cueVideoByUrl(vars: CueVideoByUrlVars): void;

    loadVideoByUrl(mediaContentUrl: string, startSeconds?: number): void;
    loadVideoByUrl(vars: CueVideoByUrlVars): void;

    cuePlaylist(list: string | string[], index?: number, startSeconds?: number): void;
    cuePlaylist(vars: PlaylistVars): void;

    loadPlaylist(list: string | string[], index?: number, startSeconds?: number): void;
    loadPlaylist(vars: PlaylistVars): void;

    /* ─────────────────── Playback control ─────────────────── */
    playVideo(): void;
    pauseVideo(): void;
    stopVideo(): void;
    seekTo(seconds: number, allowSeekAhead: boolean): void;
    clearVideo(): void;

    nextVideo(): void;
    previousVideo(): void;
    playVideoAt(index: number): void;

    /* ───────────────────── Volume / rate ───────────────────── */
    mute(): void;
    unMute(): void;
    isMuted(): boolean;
    setVolume(volume: number): void;
    getVolume(): number;

    getPlaybackRate(): number;
    setPlaybackRate(rate: number): void;
    getAvailablePlaybackRates(): number[];

    /* ───────────────────────── Size ───────────────────────── */
    setSize(width: number, height: number): { width: number; height: number };

    /* ───────────────────── Loop / shuffle ──────────────────── */
    setLoop(loopPlaylists: boolean): void;
    setShuffle(shufflePlaylist: boolean): void;

    /* ──────────────────── State inspection ─────────────────── */
    getVideoLoadedFraction(): number;
    getPlayerState(): PlayerState;
    getCurrentTime(): number;
    getDuration(): number;

    getPlaybackQuality(): string;
    setPlaybackQuality(suggestedQuality: string): void;
    getAvailableQualityLevels(): string[];

    getVideoUrl(): string;
    getVideoEmbedCode(): string;

    getPlaylist(): string[];
    getPlaylistIndex(): number;

    getVideoData(): VideoData;

    /* ─────────────── 360° video helpers (if any) ───────────── */
    getSphericalProperties(): SphericalProperties;
    setSphericalProperties(props: Partial<SphericalProperties>): void;

    /* ────────────────────── DOM / misc. ───────────────────── */
    getIframe(): HTMLIFrameElement;
    destroy(): void;

    getOption(module: string, property: string): any;
    setOption(module: string, property: string, value: any): void;

    /* ─────────────────── Event helpers ────────────────────── */
    addEventListener<T extends keyof PlayerEventMap>(
      event: T,
      listener: (e: PlayerEventMap[T]) => void
    ): void;
    removeEventListener<T extends keyof PlayerEventMap>(
      event: T,
      listener: (e: PlayerEventMap[T]) => void
    ): void;
  }

  /* ────────────────────── Constructor types ───────────────────── */
  interface PlayerOptions {
    width?: number | string;
    height?: number | string;
    videoId?: string;
    playerVars?: Record<string, string | number | boolean>;
    events?: Partial<PlayerEventHandlers>;
  }
  interface CueVideoByIdVars   { videoId: string; startSeconds?: number; endSeconds?: number; }
  interface CueVideoByUrlVars  { mediaContentUrl: string; startSeconds?: number; endSeconds?: number; }
  interface PlaylistVars       { list: string | string[]; listType?: "playlist" | "user_uploads";
                                 index?: number; startSeconds?: number; }

  /* ────────────────────  Event signatures  ───────────────────── */
  interface OnReadyEvent                 { target: Player; }
  interface OnStateChangeEvent           { target: Player; data: PlayerState; }
  interface OnPlaybackQualityChangeEvent { target: Player; data: string; }
  interface OnPlaybackRateChangeEvent    { target: Player; data: number; }
  interface OnErrorEvent                 { target: Player; data: PlayerError; }
  interface OnApiChangeEvent             { target: Player; }

  interface PlayerEventHandlers {
    onReady:                 (e: OnReadyEvent) => void;
    onStateChange:           (e: OnStateChangeEvent) => void;
    onPlaybackQualityChange: (e: OnPlaybackQualityChangeEvent) => void;
    onPlaybackRateChange:    (e: OnPlaybackRateChangeEvent) => void;
    onError:                 (e: OnErrorEvent) => void;
    onApiChange:             (e: OnApiChangeEvent) => void;
  }

  type PlayerEventMap = PlayerEventHandlers;

  /* ────────────────────  Enums & value objects  ───────────────── */
  enum PlayerState { UNSTARTED = -1, ENDED = 0, PLAYING = 1, PAUSED = 2, BUFFERING = 3, CUED = 5 }
  enum PlayerError { INVALID_PARAM = 2, HTML5_ERROR = 5, VIDEO_NOT_FOUND = 100, EMBEDDING_NOT_ALLOWED1 = 101, EMBEDDING_NOT_ALLOWED2 = 150 }

  interface SphericalProperties { yaw: number; pitch: number; roll: number; fov: number; }
  interface VideoData { video_id: string; author: string; title: string; video_quality: string; }
}