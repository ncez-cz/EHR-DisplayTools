const webpack = require("webpack");
const terserPlugin = require("terser-webpack-plugin");
const cssMinimizerPlugin = require("css-minimizer-webpack-plugin");

module.exports = env => ({
    devtool: false,
    mode: env.development ? "development" : "production",
    module: {
        rules: [
            {
                test: /\.tsx?$/,
                use: "ts-loader",
                exclude: /node_modules/,
            },
            {
                test: /\.s[ac]ss$/i,
                exclude: /node_modules/,
                type: "asset/resource",
                generator: {
                    filename: "[name].css"
                },
                use: [
                    {
                        loader: "sass-loader",
                        options: {
                            sassOptions: {
                                quietDeps: true
                            }
                        }
                    }
                ],
            }
        ],
    },
    optimization: {
        minimize: !env.development,
        minimizer: [new terserPlugin({
            parallel: true,
            terserOptions: {
                format: {
                    comments: false,
                },
            },
            extractComments: false,
        }), new cssMinimizerPlugin({parallel: true})],
    },
    resolve: {
        extensions: [".tsx", ".ts", ".js"],
    },
    plugins: [
        new webpack.optimize.LimitChunkCountPlugin({
            maxChunks: 1,
        }),
    ],
});
