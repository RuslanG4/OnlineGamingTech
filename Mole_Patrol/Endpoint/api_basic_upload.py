import csv
from flask import request, Blueprint, jsonify
import logging

api_basic_upload = Blueprint('api_basic_upload', __name__)

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Function to append data to a CSV file
def append_dict_to_csv(file_path, dict_data):
    # Flatten the dictionary if it's nested

    print(dict_data)
    
    # Check if the file exists
    try:
        with open(file_path, 'x', newline='') as csvfile:
            writer = csv.DictWriter(csvfile, fieldnames=dict_data.keys())
            writer.writeheader()
            writer.writerow(dict_data)
    except FileExistsError:
        # File exists, append the data
        with open(file_path, 'a', newline='') as csvfile:
            writer = csv.DictWriter(csvfile, fieldnames=dict_data.keys())
            writer.writerow(dict_data)

@api_basic_upload.route('/upload_data', methods=['POST'])
def post_upload():
    try:
        data = request.get_json()

        if not data:
            logger.error("No data received in the request.")
            return jsonify({'error': 'No data provided'}), 400

        # If this data exists its runtime data
        if "enemiesSpawned" in data or "enemiesKilled" in data:
            # Runtime Data (gameplay updates)
            append_dict_to_csv("runtime_data.csv", data)
            logger.info(f"Runtime data written: {data}")
        else:
            # Static Data (game settings)
            append_dict_to_csv("static_game_data.csv", data)
            logger.info(f"Static game data written: {data}")

        return jsonify({'status': 'success'}), 200

    except ValueError as ve:
        logger.error(f"ValueError: {ve}")
        return jsonify({'error': 'Invalid JSON format'}), 400
    except Exception as e:
        logger.error(f"Unexpected error: {e}")
        return jsonify({'error': 'Internal Server Error'}), 500

