export const selectVideoId = (url: string) => {
  const urlParts = new URL(url);
  if (urlParts.pathname.startsWith("/shorts")) {
    const shortUrlParts = url.split("/");
    return shortUrlParts[shortUrlParts.length - 1].split("?")[0];
  } else if (urlParts.pathname.startsWith("/watch")) {
    return url.split("v=")[1].split("&")[0];
  }
  return null;
};
