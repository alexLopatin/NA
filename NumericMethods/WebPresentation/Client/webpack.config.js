const path = require("path");

console.log(path.resolve(__dirname, 'wywwroot/js'));

module.exports = {
	module: {
		rules: [
			{
				test: /\.(js|jsx)$/,
				exclude: /node_modules/,
				use: {
					loader: "babel-loader"
				}
			}
		]
	},
	output: {
		path: path.resolve(__dirname, 'wwwroot/js'),
		filename: "index.js",
		library: "chartjs"
	}
};