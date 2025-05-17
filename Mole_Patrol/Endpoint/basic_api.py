import sys

from flask import Flask

from api_basic_upload import api_basic_upload
from flask_cors import CORS
import os

def create_app():
	app = Flask(__name__)
	app.secret_key = '_6#y2L"F4Qdkslppwkwn8z\ndkdn\xec]/'
	CORS(app, resources={r"/upload_data": {"origins": "*"}})
	app.register_blueprint(api_basic_upload)
	return app

if __name__ == "__main__":
	app = create_app()
	app.run(host='0.0.0.0', debug=True, use_reloader=True)
