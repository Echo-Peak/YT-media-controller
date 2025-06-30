const webpack = require("webpack");
const path = require("path");
const CopyPlugin = require("copy-webpack-plugin");

module.exports = {
  entry: {
    background: "./src/background.ts",
    contentScript: "./src/contentScript.ts",
  },
  output: {
    path: path.resolve(__dirname, "../../dist/browser-extension-unpacked"),
    filename: "[name].js",
  },
  optimization: {
    splitChunks: {
      name: "vendor",
      chunks(chunk) {
        return chunk.name !== "background";
      },
    },
  },
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: "ts-loader",
        exclude: /node_modules/,
      },
    ],
  },
  resolve: {
    extensions: [".ts", ".tsx", ".js"],
  },
  plugins: [
    new CopyPlugin({
      patterns: [
        { from: "./src/*.json", to: "./[name][ext]" },
        { from: "./src/assets", to: "./assets" },
      ],
      options: {},
    }),
  ],
};
