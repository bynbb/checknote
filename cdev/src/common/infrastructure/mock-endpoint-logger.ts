export class MockEndpointLogger {
  log(message: string): void {
    console.info(`[mock endpoint] ${message}`);
  }
}
