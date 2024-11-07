import { ref } from "vue";
import { useClient, useFormatters } from "@servicestack/vue";
// import { CreateMessage, UpdateMessage } from "dtos.mjs"

export default {
    template: /*html*/`
    <auto-query-grid type="Message" :visible-from="{ id:'sm', sms:'lg', status:'md', receiver:'sm', messageTemplateId:'md', partnerId:'lg', providerId:'lg', requestDate:'sm', sentDate:'md', responseDate:'md', telco:'lg', responseMassage:'lg', messageId:'sm' }">
        
        <template #id="{ id }">
            <span v-html="id"></span>
        </template>

        <template #sms="{ sms }">
            <span v-html="sms"></span>
        </template>

        <template #status-header>Trạng thái</template>
        <template #status="{ status }">
            <span :class="statusClass(status)">{{ statusMap[status] || status }}</span>
        </template>

        <template #receiver="{ receiver }">
            <span v-html="receiver"></span>
        </template>

        <template #messageTemplateId-header>
            Template
        </template>
        <template #messageTemplateId="{ messageTemplateId }">
            <span v-html="messageTemplateId"></span>
        </template>

        <template #partnerId-header>
            Partner
        </template>
        <template #partnerId="{ partnerId }">
            <span v-html="partnerId"></span>
        </template>

        <template #providerId-header>
            Provider
        </template>
        <template #providerId="{ providerId }">
            <span v-html="providerId"></span>
        </template>

        <template #requestDate="{ requestDate }">
            <span v-html="formatDateTime(requestDate)"></span>
        </template>

        <template #sentDate="{ sentDate }">
            <span v-html="formatDateTime(sentDate)"></span>
        </template>

        <template #responseDate="{ responseDate }">
            <span v-html="formatDateTime(responseDate)"></span>
        </template>

        <template #telco="{ telco }">
            <span v-html="telco"></span>
        </template>

        <template #responseMassage="{ responseMassage }">
            <span v-html="responseMassage"></span>
        </template>

        <template #messageId="{ messageId }">
            <span v-html="messageId"></span>
        </template>
    </auto-query-grid> 
  `,
    setup() {
        const client = useClient();
        const formatDateTime = (value) => {
            if (!value) return '';

            const date = new Date(value);

            const timeZoneOffset = 7 * 60 * 60000; // Asia/Ho_Chi_Minh có UTC+7
            const localTime = new Date(date.getTime() + timeZoneOffset);

            return new Intl.DateTimeFormat('vi-VN', {dateStyle: 'medium', timeStyle: 'short'}).format(localTime)
        };

        const formatDateTimeLocal = (value) => {
            if (!value) return '';

            const date = new Date(value);

            const localTime = new Date(date.getTime());

            return new Intl.DateTimeFormat('vi-VN', {dateStyle: 'medium', timeStyle: 'short'}).format(localTime)
        };

        const statusMap = {
            "Initial": "Khởi tạo",
            "Sent": "Đã gửi",
            "Delivered": "Đã nhận",
            "Failed": "Thất bại"
        };

        const statusClass = (status) => {
            return {
                "text-gray-500": status === "Initial",
                "text-blue-500": status === "Sent",
                "text-green-500": status === "Delivered",
                "text-red-500": status === "Failed"
            };
        };

        const close = () => importFile.value = null

        return {
            formatDateTime,
            statusMap,
            close,
            formatDateTimeLocal,
            statusClass
        }
    }
}