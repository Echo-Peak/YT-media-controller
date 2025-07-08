export function convertDurationToSeconds(duration: string): number {
  const parts = duration.split(":").map(Number).reverse();
  let seconds = 0;
  if (parts.length > 0) seconds += parts[0];
  if (parts.length > 1) seconds += parts[1] * 60;
  if (parts.length > 2) seconds += parts[2] * 3600;
  return seconds;
}

export function convertSecondsToDuration(seconds: number): string {
  const hrs = Math.floor(seconds / 3600);
  const mins = Math.floor((seconds % 3600) / 60);
  const secs = seconds % 60;

  if (hrs > 0) {
    return `${hrs}:${mins.toString().padStart(2, "0")}:${secs.toString().padStart(2, "0")}`;
  } else {
    return `${mins}:${secs.toString().padStart(2, "0")}`;
  }
}
