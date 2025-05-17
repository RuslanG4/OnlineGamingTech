import os
import re

# Directory containing Unity C# scripts
UNITY_PROJECT_PATH = "."

# Define regex patterns for class elements
CLASS_PATTERN = re.compile(r'class\s+(\w+)')
FIELD_PATTERN = re.compile(r'\b(public|private|protected|internal)?\s*(\w+)\s+(\w+);')  # Fields
METHOD_PATTERN = re.compile(r'\b(public|private|protected|internal|static)?\s*(\w+)\s+(\w+)\(.*?\)')  # Methods
USAGE_PATTERN = re.compile(r'\b(\w+)\s+\w+;')  # Class dependencies

# List of unwanted dependencies to exclude
EXCLUDED_TYPES = {"GameObject", "return", "using", "Vector3", "Transform", "int", "float", "string", "bool", "yield"}

# Dictionary to store class data
class_data = {}

def process_file(filepath):
    """Extracts class structure from a C# file."""
    with open(filepath, 'r', encoding='utf-8', errors='replace') as file:
        content = file.read()

    class_match = CLASS_PATTERN.search(content)
    if class_match:
        class_name = class_match.group(1)
        class_data[class_name] = {"fields": [], "methods": [], "collaborators": set()}

        # Extract fields
        for match in FIELD_PATTERN.findall(content):
            class_data[class_name]["fields"].append(f"{match[1]} {match[2]}")  # Type + Field Name

        # Extract methods
        for match in METHOD_PATTERN.findall(content):
            class_data[class_name]["methods"].append(f"{match[2]}()")  # Method Name

        # Extract collaborators (class dependencies)
        for usage in USAGE_PATTERN.findall(content):
            if usage not in EXCLUDED_TYPES and usage != class_name:
                class_data[class_name]["collaborators"].add(usage)

def scan_unity_project():
    """Scans all C# files in the Unity project directory."""
    for root, _, files in os.walk(UNITY_PROJECT_PATH):
        for file in files:
            if file.endswith(".cs"):
                process_file(os.path.join(root, file))

def generate_class_diagram():
    """Generates a PlantUML class diagram."""
    plantuml_code = "@startuml\n"

    for class_name, data in class_data.items():
        plantuml_code += f"class {class_name} {{\n"
        
        # Add fields
        for field in data["fields"]:
            plantuml_code += f"  - {field}\n"
        
        # Add methods
        for method in data["methods"]:
            plantuml_code += f"  + {method}\n"
        
        plantuml_code += "}\n"

        # Add relationships
        for collab in data["collaborators"]:
            plantuml_code += f"{class_name} --> {collab}\n"

    plantuml_code += "@enduml"

    with open("class_diagram.puml", "w", encoding="utf-8") as file:
        file.write(plantuml_code)

    print("Class diagram generated: class_diagram.puml")

# Run the script
scan_unity_project()
generate_class_diagram()
