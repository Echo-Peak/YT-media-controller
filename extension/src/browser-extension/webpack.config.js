const webpack = require("webpack");
const path = require("path");
const CopyPlugin = require("copy-webpack-plugin");

module.exports = {
    entry: {
        background: './background.ts',
        contentScript: './contentScript.ts',
    },
    output: {
        path: path.resolve(__dirname, '../../', "build"),
        filename: "[name].js",
    },
    optimization: {
        splitChunks: {
            name: "vendor",
            chunks(chunk) {
                return chunk.name !== 'background';
            }
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
                { from: "./*.json", to: "./[name][ext]" },
                { from: "./assets", to: "./assets" },
                { from: "./mobilePlugin", to: "./mobilePlugin" },
            ],
            options: {},
        }),
    ],
};