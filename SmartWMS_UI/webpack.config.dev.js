/* eslint-disable */
const path = require('path');
const webpack = require('webpack');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');

module.exports = {
  mode: 'development',
  devtool: 'eval-source-map',
  entry: './src/main.tsx',
  output: {
    path: path.resolve(__dirname, 'dist'),
    filename: '[contenthash].bundle.js',
    publicPath: '/',
  },
  resolve: {
    extensions: ['.ts', '.tsx', '.js', '.jsx', '.json'],
    alias: {
      '@': path.resolve(__dirname, 'src'),
      '@core': path.resolve(__dirname, 'src/modules/@core'),
      '@operations': path.resolve(__dirname, 'src/modules/@operations'),
      '@inventory': path.resolve(__dirname, 'src/modules/@inventory'),
      '@orders': path.resolve(__dirname, 'src/modules/@orders'),
      '@delivery': path.resolve(__dirname, 'src/modules/@delivery'),
      '@equipment': path.resolve(__dirname, 'src/modules/@equipment'),
      '@configuration': path.resolve(__dirname, 'src/modules/@configuration'),
      '@sync': path.resolve(__dirname, 'src/modules/@sync'),
      '@reports': path.resolve(__dirname, 'src/modules/@reports'),
    },
  },
  module: {
    rules: [
      {
        test: /\.(ts|tsx)$/,
        exclude: /node_modules/,
        use: {
          loader: 'ts-loader',
          options: {
            transpileOnly: true,
          },
        },
      },
      {
        test: /\.css$/,
        use: [MiniCssExtractPlugin.loader, 'css-loader'],
      },
      {
        test: /\.scss$/,
        use: [
          MiniCssExtractPlugin.loader,
          'css-loader',
          {
            loader: 'sass-loader',
            options: {
              api: 'modern-compiler',
              sassOptions: {
                silenceDeprecations: ['import'],
              },
            },
          },
        ],
      },
      {
        test: /\.svg$/,
        use: ['@svgr/webpack'],
      },
      {
        test: /\.(png|jpg|jpeg|gif|webp)$/,
        type: 'asset/resource',
      },
    ],
  },
  plugins: [
    new HtmlWebpackPlugin({
      template: 'index.html',
    }),
    new MiniCssExtractPlugin({
      filename: '[contenthash].style.css',
    }),
    new webpack.DefinePlugin({
      'process.env.NODE_ENV': JSON.stringify('development'),
      'process.env.API_BASE_URL': JSON.stringify('http://localhost:5050/api'),
      'process.env.API_URL': JSON.stringify('http://localhost:5050'),
      __BUILD_TIME__: JSON.stringify(new Date().toISOString()),
    }),
  ],
  devServer: {
    host: 'localhost',
    port: 3040,
    static: {
      directory: path.join(__dirname, 'public'),
    },
    hot: false,
    liveReload: false,
    historyApiFallback: true,
    client: {
      overlay: true,
    },
  },
};
