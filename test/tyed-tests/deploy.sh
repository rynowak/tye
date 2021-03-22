#!/bin/bash
set -e

if [[ -z "$SUBSCRIPTION_ID" ]]
then
    echo "SUBSCRIPTION_ID not set"
fi

if [[ -z "$RESOURCE_GROUP" ]]
then
    echo "RESOURCE_GROUP not set"
fi

URL_BASE="http://localhost:5000/subscriptions/$SUBSCRIPTION_ID/resourceGroups/$RESOURCE_GROUP/providers/Radius.Tye/Containers"
BASE_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

http PUT \
    "$URL_BASE/frontend" \
    "@$BASE_DIR/container-frontend.json" \
    --check-status
http PUT \
    "$URL_BASE/backend" \
    "@$BASE_DIR/container-backend.json" \
    --check-status