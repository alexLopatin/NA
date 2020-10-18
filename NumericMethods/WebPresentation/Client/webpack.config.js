const path = require("path");

module.exports = {
	module: {
		rules: [
			{
				test: /\.(js|jsx)$/,
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