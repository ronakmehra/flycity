const path = require('path');
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const HtmlWebpackPlugin = require('html-webpack-plugin');

const mode = process.env.NODE_ENV || 'development';
const prod = mode === 'production';

const urlPath = `/`

module.exports = {
    entry: {
        main: './src/main.js'
    },
    resolve: {
        alias: {
            "svelte": path.resolve('node_modules', 'svelte'),
            "@": path.resolve(__dirname, './src'),
            "api": path.resolve(__dirname, './src/api'),
            "store": path.resolve(__dirname, './src/store'),
            "components": path.resolve(__dirname, './src/components'),
            "router": path.resolve(__dirname, './src/router/index.js'),
            "json": path.resolve(__dirname, './src/json'),
            "lang": path.resolve(__dirname, './lang')
        },
        conditionNames: ['svelte', 'browser', 'import'],
        extensions: ['.mjs', '.js', '.svelte'],
        mainFields: ['svelte', 'browser', 'module', 'main'],
        fallback: {
            "querystring": require.resolve("querystring"),
            "url": require.resolve("url"),
        },
    },
    output: {
        path: path.resolve(__dirname, "./../client_packages/interface"),
		filename: `build/bundle.js`,
        libraryTarget: "umd",
    },
    plugins: [
        new HtmlWebpackPlugin({
            template: "./src/index.html",
            title: `FlyCity Roleplay - ${new Date()}`,
            filename: './index.html',
            inject: false,
        }),
        new MiniCssExtractPlugin({ 
            filename: `build/bundle.css`
        })
    ],
    module: {
        rules: [
            {
				test: /\.svelte$/,
				use: {
					loader: 'svelte-loader',
					options: {
						emitCss: true,
						hotReload: !prod
					}
				}
            },
            {
                test: /node_modules\/svelte\/.*\.mjs$/,
                resolve: {
                    fullySpecified: false,
                },
            },
            {
                test: /\.(c|sac|sa|sc)ss$/i,
                enforce: "pre",
                use: [
                    {
                        loader: MiniCssExtractPlugin.loader,
                        options: {
                            publicPath: '../',
                        }
                    },
                    "css-loader", {
                        loader: "postcss-loader",
                        options: {
                            postcssOptions: {
                                plugins: [require("autoprefixer")]
                            }
                        }
                    },
                    {
                        loader: "sass-loader",
                        options: {
                            api: "legacy"
                        }
                    }
                ]
            },
            {
                test: /\.(jpe?g|png|svg?|gif)$/i,
                use: [{
                    loader: 'file-loader',
                    options: {
                        esModule: false,
                        name: '[path]/[name].[ext]',
                        publicPath: (url, resourcePath, context) => {
                            url = url.split('/').filter(x => x).join('/');
                            return urlPath + url;
                        }
                    }
                }]
            },
            {
                test: /\.(webm|ttf|eot|woff(2)?|ogg|mp3|wav|mpe?g)(\?[a-z0-9=&.]+)?$/,
                use: [{
                    loader: 'file-loader',
                    options: {
                        name: '[path]/[name].[ext]'
                    }
                }]
			},
        ]
    },
    mode,
    devtool: prod ? false: 'source-map',
    devServer: {
        static: path.join(__dirname, 'dist'),
        compress: true,
        port: 8888
    }
}
