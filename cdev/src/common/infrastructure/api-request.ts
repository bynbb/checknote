export function isChecknoteApiRequest(url: string): boolean {
  return url === '/api' ||
    url.startsWith('/api/') ||
    /^https?:\/\/[^/]+\/api(?:\/|$)/i.test(url);
}
